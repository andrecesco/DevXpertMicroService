# 🔐 GitHub Actions - Secrets Configuration Guide

## 📋 Resumo

Este guia descreve como configurar os **Secrets** necessários para que os workflows de CI/CD funcionem corretamente.

---

## 🔑 Secrets Obrigatórios

### 1. **DOCKER_USERNAME** e **DOCKER_PASSWORD**
Credenciais do Docker Hub para autenticação e push de imagens.

**Onde configurar:**
1. Vá para: `Settings` → `Secrets and variables` → `Actions`
2. Clique em "New repository secret"
3. Adicione:
   - **Name:** `DOCKER_USERNAME`
   - **Value:** seu username Docker Hub
4. Repita para `DOCKER_PASSWORD` com seu token de acesso

**Como gerar o token:**
- Acesse: https://hub.docker.com/settings/security
- Clique em "New Access Token"
- Selecione permissões: Read & Write
- Copie o token

---

## 🔐 Secrets Opcionais (Recomendados)

### 2. **SONAR_TOKEN** (SonarQube/SonarCloud)
Para análise de qualidade de código.

**Setup:**
1. Registre-se em: https://sonarcloud.io (grátis para open source)
2. Crie um projeto para seu repositório
3. Gere um token em: Profile → Security → Tokens
4. Adicione o secret `SONAR_TOKEN` no GitHub

### 3. **SNYK_TOKEN** (Snyk)
Para scanning de vulnerabilidades em dependências.

**Setup:**
1. Registre-se em: https://snyk.io
2. Conecte sua conta GitHub
3. Gere um token em: Settings → API Token
4. Adicione o secret `SNYK_TOKEN` no GitHub

### 4. **GITHUB_TOKEN** (Automático)
Já vem pré-configurado no GitHub Actions.
- Usado para criar releases, comentários em PRs, etc.

---

## 📝 Configuração por Workflow

### CI Workflow (`.github/workflows/ci.yml`)
```yaml
Secrets necessários: Nenhum obrigatório
Secrets opcionais:  SONAR_TOKEN (para SonarQube)
					SNYK_TOKEN (para análise de vulnerabilidades)
```

**Acionadores:**
- Push em `develop`
- Pull Request para `develop` ou `main`

---

### CD Workflow (`.github/workflows/cd.yml`)
```yaml
Secrets necessários: DOCKER_USERNAME
					 DOCKER_PASSWORD
Secrets opcionais:   Nenhum
```

**Acionadores:**
- Push em `main` (branch de produção)
- Tags `v*.*.*` (versionamento semântico)
- Manual via `workflow_dispatch`

---

### Security Workflow (`.github/workflows/security.yml`)
```yaml
Secrets necessários: Nenhum obrigatório
Secrets opcionais:  SNYK_TOKEN (para Snyk)
```

**Acionadores:**
- Push em `main` ou `develop`
- Pull Requests
- Schedule: Semanalmente (domingo 2 AM UTC)

---

### PR Workflow (`.github/workflows/pr.yml`)
```yaml
Secrets necessários: Nenhum
Secrets opcionais:   Nenhum
```

**Acionadores:**
- Abertura de Pull Requests
- Sincronização de PRs
- Marked as ready for review

---

## 🚀 Passo a Passo - Setup Completo

### 1️⃣ Docker Hub Credentials (Obrigatório)

```bash
# 1. Acesse: https://hub.docker.com/settings/security
# 2. Clique "New Access Token"
# 3. Nome: "GitHub Actions"
# 4. Read & Write permissions
# 5. Generate e copie
```

```bash
# 6. No GitHub, vá para Settings → Secrets and variables → Actions
# 7. New repository secret
Name:  DOCKER_USERNAME
Value: seu-username

# 8. Novo secret
Name:  DOCKER_PASSWORD
Value: seu-token-token
```

### 2️⃣ SonarCloud (Recomendado)

```bash
# 1. Acesse: https://sonarcloud.io/
# 2. Sign up with GitHub
# 3. Clique "Create new project"
# 4. Selecione seu repositório
# 5. Escolha "GitHub Actions" como CI
# 6. Settings → User → Security → Generate token
```

```bash
# 7. No GitHub, adicione secret:
Name:  SONAR_TOKEN
Value: seu-token-sonar
```

### 3️⃣ Snyk (Opcional)

```bash
# 1. Acesse: https://snyk.io/
# 2. Sign up with GitHub
# 3. Integre seu repositório
# 4. Settings → API Token → Copy
```

```bash
# 5. No GitHub, adicione secret:
Name:  SNYK_TOKEN
Value: seu-token-snyk
```

---

## ⚙️ Variáveis de Ambiente

Além de Secrets, você pode usar **Variables** para configurações não-sensíveis:

```bash
# Settings → Secrets and variables → Variables

# Nome: REGISTRY
# Valor: docker.io

# Nome: DOTNET_VERSION
# Valor: 10.0.x
```

---

## 🔍 Verificar Status dos Workflows

1. Vá para: `Actions` (abas no repositório)
2. Veja lista de workflows
3. Clique em um workflow para ver execução
4. Visualize logs completos para debug

---

## 🐛 Troubleshooting

### ❌ "Authentication failed"
**Solução:** Verificar se `DOCKER_USERNAME` e `DOCKER_PASSWORD` estão corretos

### ❌ "Dockerfile not found"
**Solução:** Verificar se paths dos Dockerfiles estão corretos em `cd.yml`

### ❌ "SONAR_TOKEN not set"
**Solução:** Adicionar secret `SONAR_TOKEN` no GitHub (pode ser opcional se comentar em `ci.yml`)

### ❌ "Timeout during build"
**Solução:** 
- Aumentar timeout (padrão 360 minutos)
- Usar cache Docker (`docker/build-push-action`)
- Paralelizar builds (já implementado com matrix)

---

## 📊 Matriz de Compatibilidade

| Secret | CI | CD | Security | PR | Obrigatório |
|--------|----|----|----------|----|----|
| DOCKER_USERNAME | ❌ | ✅ | ❌ | ❌ | ✅ (CD) |
| DOCKER_PASSWORD | ❌ | ✅ | ❌ | ❌ | ✅ (CD) |
| SONAR_TOKEN | ✅* | ❌ | ❌ | ❌ | ❌ |
| SNYK_TOKEN | ✅* | ❌ | ✅* | ❌ | ❌ |
| GITHUB_TOKEN | ✅ | ✅ | ✅ | ✅ | ✅ (auto) |

*Opcional, recomendado para funcionalidade completa

---

## 🔐 Best Practices de Segurança

1. ✅ **Use Access Tokens, não passwords**
   - Mais seguro, revogável, limitado por escopo

2. ✅ **Rotacione secrets regularmente**
   - A cada 3-6 meses

3. ✅ **Nunca commite secrets no repositório**
   - Use `.gitignore` para arquivos locais

4. ✅ **Limite permissões ao mínimo necessário**
   - Docker Hub: apenas Read & Write para seu namespace

5. ✅ **Use branch protection rules**
   - Require status checks before merging

6. ✅ **Monitore logs de execução**
   - GitHub Actions → Actions → Ver logs

---

## 📚 Referências

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Docker Hub Access Tokens](https://docs.docker.com/docker-hub/access-tokens/)
- [SonarCloud Setup](https://docs.sonarcloud.io/getting-started/github/)
- [Snyk GitHub Integration](https://docs.snyk.io/integrations/ci-cd-integrations/github-actions-for-snyk)

---

## 🎯 Checklist Final

- [ ] ✅ `DOCKER_USERNAME` configurado
- [ ] ✅ `DOCKER_PASSWORD` configurado
- [ ] [ ] `SONAR_TOKEN` configurado (opcional)
- [ ] [ ] `SNYK_TOKEN` configurado (opcional)
- [ ] ✅ Workflows estão em `.github/workflows/`
- [ ] ✅ Dockerfiles existem em todos os caminhos especificados
- [ ] ✅ `docker-compose.yml` está no root
- [ ] ✅ `.dockerignore` está no root
- [ ] ✅ Teste um push em `develop` para ativar CI
- [ ] ✅ Teste um PR para `main` para verificar validações

---

**Quando tudo estiver configurado, os workflows rodarão automaticamente!** 🚀

Dúvidas? Verifique os logs em `Actions` → `Workflow name` → `Run logs`
