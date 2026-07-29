# Índice de Documentação — EduOnline

Este arquivo é o **ponto de entrada oficial** para a documentação do repositório.

## Documentos principais

- [../README.md](../README.md) — visão geral do projeto, execução com Docker Compose e Kubernetes, endpoints e troubleshooting.
- [docker.md](./docker.md) — guia detalhado de execução e operação local com Docker/Compose.
- [ci-cd-testing-guide.md](./ci-cd-testing-guide.md) — validação da esteira e testes do workflow atual.
- [observability-validation-runbook.md](./observability-validation-runbook.md) — checklist operacional de observabilidade e validação pós-deploy.
- [SECURITY-COMPLIANCE-MATRIX.md](./SECURITY-COMPLIANCE-MATRIX.md) — matriz de segurança com distinção entre itens concluídos, parciais e não aplicados.
- [projeto-mod05.md](./projeto-mod05.md) — enunciado/requisitos acadêmicos do módulo.

## Infraestrutura complementar

- [../infra/ARCHITECTURE.md](../infra/ARCHITECTURE.md) — arquitetura de referência da plataforma.
- [../infra/kubernetes/README.md](../infra/kubernetes/README.md) — manifests e scripts Kubernetes.
- Guias de segurança em `infra/kubernetes/security/**` — material complementar para experimentação e bootstrap local (não substitui a matriz de status em `docs/SECURITY-COMPLIANCE-MATRIX.md`).

## Regra de manutenção

Para evitar sobreposição e divergência:

1. Atualize este índice ao criar/remover guias.
2. Mantenha o `README.md` raiz como resumo executivo.
3. Registre estado real (operante/parcial/não aplicado) na matriz de segurança.
