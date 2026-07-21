# Resolução de Feedbacks

Este arquivo consolida as melhorias aplicadas em resposta aos feedbacks recebidos nas avaliações parciais e finais do curso.

---

## Módulo 4 — Feedback Final (resolvido antes da entrega do Módulo 5)

| Apontamento | Resolução |
|-------------|-----------|
| Arquivos acessórios versionados (`fact_aaa.snippet`, `test-output.txt`, `test-output-full.txt`) | Removidos do controle de versão |
| Segredo JWT com fallback hardcoded (`ChaveSuperSecreta` / `?? "..."`) | Removido; segredos passaram a vir exclusivamente de configuração |
| Código comentado em arquivos `.cs` | Removido no commit `2ca7d93` ("removendo códigos comentados e rodando clean up") |
| Testes ausentes em `EduOnline.Alunos.IntegrationTest`, `EduOnline.Conteudos.UnitTest` e `EduOnline.Conteudos.IntegrationTest` (reportavam "No test is available") | Testes implementados; projetos passaram a ser reconhecidos pelo runner |

---

## Módulo 5 — Feedback Parcial (resolvido nesta série de branches)

### Organização e Estrutura do Projeto

| Apontamento | Branch | PR | Resolução |
|-------------|--------|----|-----------|
| Duplicação massiva da árvore de infraestrutura: `infra/security/` e `infra/observability/` eram cópias órfãs de `infra/kubernetes/security/` e `infra/kubernetes/observability/` (~40 YAMLs duplicados) | `fix/infra-cleanup` | #23 | `infra/security/` removida inteiramente (28 arquivos); `infra/observability/` parcialmente removida (42 arquivos), mantidos apenas `otel-collector-config.yaml` e `prometheus/prometheus-local.yml` usados pelo `docker-compose.yml` |
| Vault presente em três locais distintos (`infra/kubernetes/vault/`, `infra/kubernetes/security/vault/`, `infra/security/vault/`) | `fix/infra-cleanup` | #23 | Consolidado em um único local canônico: `infra/kubernetes/security/vault/` (referenciado por `security/kustomization.yaml`); as outras duas cópias foram removidas |
| `pod-security-policies.yaml` usa `kind: PodSecurityPolicy`, API removida do Kubernetes desde a versão 1.25 — manifesto morto | `fix/infra-remove-pod-security-policy` | #24 | Manifesto, guia (`PSP-GUIDE.md`) e script (`apply-psp.sh`) removidos; `security/kustomization.yaml` atualizado; `namespace.yaml` elevado de PSA `baseline` para `restricted` (todos os Deployments já cumpriam os requisitos) |
| Documentação dispersa e redundante na raiz: `DOCKER-README.md`, `CI-CD-TESTING-GUIDE.md`, `ProjetoMod05.md` | `fix/docs-consolidation` | #25 | Movidos para `docs/`: `docs/docker.md`, `docs/ci-cd-testing-guide.md`, `docs/projeto-mod05.md`; `README.md` seção 10 atualizado com os novos caminhos |
| `FEEDBACK.md` vazio (0 bytes) | `fix/docs-consolidation` | #25 | Este arquivo |
| `SECURITY-COMPLIANCE-MATRIX.md` referenciava `PodSecurityPolicy` como implementada | `fix/docs-consolidation` | #25 | Linha atualizada para refletir o mecanismo atual: PSA `restricted` + `securityContext` nos Deployments |
