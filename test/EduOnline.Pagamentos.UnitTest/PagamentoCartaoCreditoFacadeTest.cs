using System;
using Moq;
using EduOnline.Pagamentos.AntiCorruption;
using EduOnline.Pagamentos.Domain;
using Xunit;

namespace EduOnline.Pagamentos.UnitTest
{
    public class PagamentoCartaoCreditoFacadeTest
    {
        private static Curso NovoCurso(decimal valor)
        {
            return new Curso
            {
                Id = Guid.NewGuid(),
                Valor = valor
            };
        }

        private static Pagamento NovoPagamento(decimal total, string? numeroCartao = "1234567890123456")
        {
            return new Pagamento
            {
                Id = Guid.NewGuid(),
                AlunoId = Guid.NewGuid(),
                Total = total,
                NomeCartao = "Teste",
                NumeroCartao = numeroCartao,
                ExpiracaoCartao = "12/29",
                CvvCartao = "123"
            };
        }

        [Fact]
        public void RealizarPagamento_DeveRetornarTransacaoAprovada_QuandoCommitRetornaTrue()
        {
            // Arrange
            var curso = NovoCurso(valor: 199.90m);
            var pagamento = NovoPagamento(total: 199.90m);

            var mockGateway = new Mock<IPayPalGateway>(MockBehavior.Strict);
            var mockConfig = new Mock<IConfigurationManager>(MockBehavior.Strict);

            var apiKey = "api-key";
            var encryptionKey = "enc-key";
            var serviceKey = "service-key";
            var cardHashKey = "card-hash";

            var sequence = new MockSequence();

            mockConfig.InSequence(sequence)
                      .Setup(x => x.GetValue("apiKey"))
                      .Returns(apiKey);
            mockConfig.InSequence(sequence)
                      .Setup(x => x.GetValue("encriptionKey"))
                      .Returns(encryptionKey);

            mockGateway.InSequence(sequence)
                       .Setup(x => x.GetPayPalServiceKey(apiKey, encryptionKey))
                       .Returns(serviceKey);

            mockGateway.InSequence(sequence)
                       .Setup(x => x.GetCardHashKey(serviceKey, pagamento.NumeroCartao!))
                       .Returns(cardHashKey);

            mockGateway.InSequence(sequence)
                       .Setup(x => x.CommitTransaction(cardHashKey, curso.Id.ToString(), pagamento.Total))
                       .Returns(true);

            var facade = new PagamentoCartaoCreditoFacade(mockGateway.Object, mockConfig.Object);

            // Act
            var transacao = facade.RealizarPagamento(curso, pagamento);

            // Assert
            Assert.NotNull(transacao);
            Assert.Equal(StatusTransacao.Aprovado.Id, transacao.StatusTransacaoId);
            Assert.Equal(curso.Valor, transacao.Total);
            Assert.Equal(pagamento.Id, transacao.PagamentoId);

            mockConfig.Verify(x => x.GetValue("apiKey"), Times.Once);
            mockConfig.Verify(x => x.GetValue("encriptionKey"), Times.Once);
            mockGateway.Verify(x => x.GetPayPalServiceKey(apiKey, encryptionKey), Times.Once);
            mockGateway.Verify(x => x.GetCardHashKey(serviceKey, pagamento.NumeroCartao!), Times.Once);
            mockGateway.Verify(x => x.CommitTransaction(cardHashKey, curso.Id.ToString(), pagamento.Total), Times.Once);
            mockGateway.VerifyNoOtherCalls();
            mockConfig.VerifyNoOtherCalls();
        }

        [Fact]
        public void RealizarPagamento_DeveRetornarTransacaoRecusada_QuandoCommitRetornaFalse()
        {
            // Arrange
            var curso = NovoCurso(valor: 100);
            var pagamento = NovoPagamento(total: 100);

            var mockGateway = new Mock<IPayPalGateway>(MockBehavior.Strict);
            var mockConfig = new Mock<IConfigurationManager>(MockBehavior.Strict);

            mockConfig.Setup(x => x.GetValue("apiKey")).Returns("api-key");
            mockConfig.Setup(x => x.GetValue("encriptionKey")).Returns("enc-key");
            mockGateway.Setup(x => x.GetPayPalServiceKey("api-key", "enc-key")).Returns("service-key");
            mockGateway.Setup(x => x.GetCardHashKey("service-key", pagamento.NumeroCartao!)).Returns("card-hash");
            mockGateway.Setup(x => x.CommitTransaction("card-hash", curso.Id.ToString(), pagamento.Total)).Returns(false);

            var facade = new PagamentoCartaoCreditoFacade(mockGateway.Object, mockConfig.Object);

            // Act
            var transacao = facade.RealizarPagamento(curso, pagamento);

            // Assert
            Assert.NotNull(transacao);
            Assert.Equal(StatusTransacao.Recusado.Id, transacao.StatusTransacaoId);
            Assert.Equal(curso.Valor, transacao.Total);
            Assert.Equal(pagamento.Id, transacao.PagamentoId);
        }

        [Fact]
        public void RealizarPagamento_DeveUsarNumeroCartaoVazio_QuandoNumeroCartaoForNulo()
        {
            // Arrange
            var curso = NovoCurso(valor: 50);
            var pagamento = NovoPagamento(total: 50, numeroCartao: null);

            var mockGateway = new Mock<IPayPalGateway>(MockBehavior.Strict);
            var mockConfig = new Mock<IConfigurationManager>(MockBehavior.Strict);

            mockConfig.Setup(x => x.GetValue("apiKey")).Returns("k1");
            mockConfig.Setup(x => x.GetValue("encriptionKey")).Returns("k2");
            mockGateway.Setup(x => x.GetPayPalServiceKey("k1", "k2")).Returns("svc");
            mockGateway.Setup(x => x.GetCardHashKey("svc", string.Empty)).Returns("hash");
            mockGateway.Setup(x => x.CommitTransaction("hash", curso.Id.ToString(), pagamento.Total)).Returns(true);

            var facade = new PagamentoCartaoCreditoFacade(mockGateway.Object, mockConfig.Object);

            // Act
            var transacao = facade.RealizarPagamento(curso, pagamento);

            // Assert
            Assert.Equal(StatusTransacao.Aprovado.Id, transacao.StatusTransacaoId);
            mockGateway.Verify(x => x.GetCardHashKey("svc", string.Empty), Times.Once);
        }
    }
}
