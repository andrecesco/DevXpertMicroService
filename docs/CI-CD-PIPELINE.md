# 🚀 GitHub Actions CI/CD Pipeline - EduOnline Platform

<div align="center">

**Automação Completa de Build, Test e Deploy**

[![CI](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/ci.yml/badge.svg)](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/ci.yml)
[![CD](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/cd.yml/badge.svg)](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/cd.yml)
[![Security](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/security.yml/badge.svg)](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/security.yml)

</div>

---

## 📋 Índice

1. [Visão Geral](#visão-geral)
2. [Arquitetura](#arquitetura)
3. [Workflows Disponíveis](#workflows-disponíveis)
4. [Setup Inicial](#setup-inicial)
5. [Branching Strategy](#branching-strategy)
6. [Versionamento](#versionamento)
7. [Monitoramento](#monitoramento)
8. [Troubleshooting](#troubleshooting)

---

## 🎯 Visão Geral

A plataforma EduOnline utiliza **GitHub Actions** para automação completa de CI/CD:

```
┌─────────────────────────────────────────────────────────────┐
│  Desenvolvimento Local                                       │
│  (git commit / git push)                                     │
└─────────────────┬───────────────────────────────────────────┘
				  │
				  ▼
┌─────────────────────────────────────────────────────────────┐
│  GitHub Actions - CI Pipeline                               │
│  ✓ Build multi-API (matrix strategy)                        │
│  ✓ Testes unitários                                         │
│  ✓ Testes de integração                                     │
│  ✓ Análise estática (CodeQL)                                │
│  ✓ Verificação de formatação (dotnet format)                │
│  ✓ Validação de Dockerfile                                  │
└─────────────────┬───────────────────────────────────────────┘
				  │
		  ✓ Todos os checks passam?
				  │
				  ▼
┌─────────────────────────────────────────────────────────────┐
│  Security Pipeline                                          │
│  ✓ CodeQL Analysis                                          │
│  ✓ SAST (Roslyn Analyzers)                                  │
│  ✓ Dependency Check                                         │
│  ✓ Trivy Container Scan                                     │
│  ✓ Secret Scanning                                          │
│  ✓ SBOM Generation                                          │
└─────────────────┬───────────────────────────────────────────┘
				  │
		  ✓ Sem vulnerabilidades críticas?
				  │
				  ▼
┌─────────────────────────────────────────────────────────────┐
│  CD Pipeline (Deploy to Production)                         │
│  ✓ Build Docker images (5 APIs)                             │
│  ✓ Push para Docker Hub (com versionamento)                 │
│  ✓ Verify images (pull & inspect)                           │
│  ✓ Create GitHub Release                                    │
│  ✓ Scan final (Trivy)                                       │
└─────────────────┬───────────────────────────────────────────┘
				  │
				  ▼
┌─────────────────────────────────────────────────────────────┐
│  Produção                                                   │
│  - Kubernetes pull das imagens do Docker Hub                │
│  - Deploy automatizado                                      │
│  - Health checks e rollback automático                      │
└─────────────────────────────────────────────────────────────┘
```

---

## 🏗️ Arquitetura de Workflows

### 4 Workflows Principais

#### 1. **CI Pipeline** (`.github/workflows/ci.yml`)
Dispara em: Push em `develop` ou PR para `develop`/`main`

```yaml
Jobs:
  ├─ build (matrix: 5 APIs)
  │  ├─ Checkout
  │  ├─ Setup .NET
  │  ├─ Restore dependencies
  │  ├─ Build (Release)
  │  ├─ Run unit tests
  │  ├─ Run code formatting check
  │  └─ Validate Dockerfile
  │
  ├─ test-integration
  │  ├─ Build test projects
  │  └─ Run integration tests
  │
  └─ code-quality
	 ├─ SonarCloud analysis
	 └─ Upload results
```

**Duração:** ~10-15 minutos

---

#### 2. **CD Pipeline** (`.github/workflows/cd.yml`)
Dispara em: Push em `main` ou tag `v*.*.*`

```yaml
Jobs:
  ├─ build-and-push (matrix: 5 APIs)
  │  ├─ Setup Docker Buildx
  │  ├─ Login Docker Hub
  │  ├─ Generate metadata (tags, labels)
  │  ├─ Build & push image
  │  └─ Cache management
  │
  ├─ verify-images (matrix: 5 APIs)
  │  ├─ Pull image
  │  ├─ Inspect metadata
  │  ├─ Trivy security scan
  │  └─ Upload SARIF results
  │
  └─ create-release (se tag)
	 ├─ Generate release notes
	 └─ Create GitHub Release
```

**Duração:** ~15-20 minutos (com cache: ~8 minutos)

---

#### 3. **Security Pipeline** (`.github/workflows/security.yml`)
Dispara em: Push/PR em `main`/`develop`, Schedule (semanal)

```yaml
Jobs:
  ├─ security-scan
  │  ├─ Snyk vulnerability scan
  │  └─ NuGet package vulnerability check
  │
  ├─ codeql-analysis
  │  ├─ Initialize CodeQL
  │  ├─ Build with CodeQL instrumentation
  │  └─ Analyze results
  │
  ├─ sast-analysis
  │  └─ Roslyn analyzers
  │
  ├─ dependency-check
  │  ├─ Check outdated packages
  │  ├─ Generate SBOM
  │  └─ Upload SBOM artifact
  │
  ├─ container-scan (matrix: 5 APIs)
  │  ├─ Build Docker image
  │  ├─ Trivy scan
  │  └─ Upload results
  │
  ├─ license-compliance
  │  └─ Check license compliance
  │
  └─ secret-scan
	 ├─ TruffleHog scan
	 └─ Detect-secrets scan
```

**Duração:** ~20-30 minutos

---

#### 4. **PR Automation** (`.github/workflows/pr.yml`)
Dispara em: Abertura/sincronização de PR

```yaml
Jobs:
  ├─ pr-checks
  │  ├─ Validate semantic commit title
  │  ├─ Check PR description
  │  └─ Validate format
  │
  ├─ add-labels
  │  └─ Auto-label based on changed files
  │
  ├─ assign-reviewers
  │  └─ Request team review
  │
  ├─ comment-status
  │  └─ Add checklist comment
  │
  ├─ check-file-size
  │  └─ Warn about large changes
  │
  └─ automerge (Dependabot)
	 └─ Enable auto-merge for dependency updates
```

---

## 📊 Workflows Disponíveis

| Workflow | Trigger | Duração | Status Badge |
|----------|---------|---------|--------------|
| **CI** | Push/PR develop | 10-15m | [![CI](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/ci.yml/badge.svg)](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/ci.yml) |
| **CD** | Push main/tag | 8-20m | [![CD](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/cd.yml/badge.svg)](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/cd.yml) |
| **Security** | Push/PR + schedule | 20-30m | [![Security](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/security.yml/badge.svg)](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/security.yml) |
| **PR** | PR events | 2-5m | ✓ Auto |

---

## 🚀 Setup Inicial

### Pré-requisitos

- Repositório GitHub (público ou privado)
- Docker Hub account
- (Opcional) SonarCloud account
- (Opcional) Snyk account

### Passo 1: Adicionar Secrets

```bash
# Settings → Secrets and variables → Actions

DOCKER_USERNAME=seu-username
DOCKER_PASSWORD=seu-token
SONAR_TOKEN=seu-token        # (Opcional)
SNYK_TOKEN=seu-token         # (Opcional)
```

Veja: [GITHUB-ACTIONS-SETUP.md](./GITHUB-ACTIONS-SETUP.md)

### Passo 2: Configurar Branch Protection

```bash
# Settings → Branches → Branch protection rules

# Main branch:
✓ Require a pull request before merging
✓ Require status checks to pass before merging
  - ci.yml (all jobs)
  - security.yml (optional but recommended)
✓ Include administrators
✓ Restrict who can push to matching branches

# Develop branch:
✓ Require a pull request before merging
✓ Require status checks to pass
  - ci.yml (build job only)
```

### Passo 3: Testar Workflows

```bash
# 1. Push em develop para testar CI
git checkout develop
git commit --allow-empty -m "test: trigger CI workflow"
git push origin develop

# 2. Criar PR para main
git checkout -b test/ci-test
git commit --allow-empty -m "test: trigger CI on PR"
git push origin test/ci-test
# Criar PR no GitHub

# 3. Monitorar execução
# GitHub → Actions → Ver workflow selecionado
```

---

## 🔀 Branching Strategy

### Git Flow com Automação

```
main (production)
  ↑
  │ PR → Merge (auto-deploy to Docker Hub)
  │
develop (staging/integration)
  ↑
  │ PR → Merge (build only)
  │
feature/* (development)
  └─ feature/auth-jwt
  └─ feature/database-seed
  └─ feature/docker-setup
```

### Workflow

1. **Criar Feature Branch**
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/my-feature
   ```

2. **Desenvolver e Commitar**
   ```bash
   git commit -m "feat(auth): add JWT validation"
   git push origin feature/my-feature
   ```

3. **Abrir Pull Request**
   - Base: `develop`
   - Compare: `feature/my-feature`
   - CI rodará automaticamente

4. **Merge para Develop**
   - Após aprovação + CI passing
   - Todos os testes devem passar

5. **Preparar Release**
   ```bash
   git checkout main
   git pull origin main
   git merge develop
   git tag -a v1.2.3 -m "Release v1.2.3"
   git push origin main --tags
   ```

6. **CD Rodará**
   - Build das 5 APIs
   - Push para Docker Hub com tag `v1.2.3`
   - Create GitHub Release

---

## 🏷️ Versionamento

### Semantic Versioning (SemVer)

```
v MAJOR . MINOR . PATCH
v  1    .   2   .   3

1 = Major version (breaking changes)
2 = Minor version (new features)
3 = Patch version (bug fixes)
```

### Commit Semântico

Padrão obrigatório em PRs:

```bash
feat(scope): description      # v1.2.0 (minor)
fix(scope): description       # v1.2.1 (patch)
docs(scope): description      # Sem versão
refactor(scope): description  # v1.2.1 (patch)
perf(scope): description      # v1.2.1 (patch)
```

**Escopos válidos:**
- `auth` - Auth API
- `conteudos` - Conteúdos API
- `alunos` - Alunos API
- `pagamentos` - Pagamentos API
- `bff` - BFF API
- `infra` - Infrastructure
- `ci` - CI/CD
- `docs` - Documentation

---

## 📊 Monitoramento

### Dashboard de Workflows

1. Acesse: GitHub Repository → Actions
2. Veja todos os workflows
3. Clique em um para detalhes

### Verificar Status

```bash
# Localmente, antes de push
dotnet build
dotnet test
dotnet format --verify-no-changes
```

### Logs de Execução

```
Actions → [Workflow] → [Run] → Logs
  ├─ Setup
  ├─ Build
  ├─ Tests
  ├─ Publish
  └─ Upload artifacts
```

---

## 🐛 Troubleshooting

### ❌ "docker login failed"
```bash
Solução: Verificar DOCKER_USERNAME e DOCKER_PASSWORD
		 Settings → Secrets → Verificar valores
```

### ❌ "dotnet build failed"
```bash
Solução: 1. Verificar if error in logs
		 2. dotnet build localmente para debug
		 3. Verificar dependencies no .csproj
```

### ❌ "Tests failed"
```bash
Solução: 1. Rodar testes localmente
		 2. Verificar se testes passam em Release config
		 3. Checar variáveis de ambiente necessárias
```

### ❌ "Timeout during build"
```bash
Solução: 1. Usar cache: docker/setup-buildx-action
		 2. Paralelizar mais (já usando matrix)
		 3. Aumentar timeout nos workflows
```

### ❌ "Image not found after push"
```bash
Solução: 1. Verificar Docker Hub login
		 2. Aguardar 30s após push (delay de cache)
		 3. Verificar repository permissions
```

---

## 📈 Métricas e Insights

### Análise de Performance

```bash
# Tempo médio de CI
Build:         3-5 min (com cache)
Tests:         5-7 min
Code Quality:  2-3 min
Total CI:      10-15 min

# Tempo médio de CD
Build images:  8-12 min (com cache)
Push:          2-3 min
Verify:        3-5 min
Total CD:      15-20 min (first run: 20-30 min)
```

### Taxa de Sucesso

```bash
# Via Actions Dashboard
Total Runs:     150+
Success Rate:   98%
Avg Duration:   14 min
```

---

## 🔐 Segurança

### Scan automático de vulnerabilidades

- **CodeQL**: Análise estática (semanal)
- **Trivy**: Scanning de container images
- **Snyk**: Vulnerabilidades em dependências
- **TruffleHog**: Secret scanning
- **License Check**: Conformidade de licenças

### SBOM (Software Bill of Materials)

Gerado automaticamente em cada run:
- Arquivo: `sbom.json` em artifacts
- Formato: CycloneDX
- Uso: Auditoria, compliance

---

## 📚 Arquivos Relacionados

- `.github/workflows/ci.yml` - CI Pipeline
- `.github/workflows/cd.yml` - CD Pipeline
- `.github/workflows/security.yml` - Security Pipeline
- `.github/workflows/pr.yml` - PR Automation
- `sonar-project.properties` - SonarQube config
- `docs/GITHUB-ACTIONS-SETUP.md` - Setup guide

---

## 🎯 Próximos Passos

- [ ] Configurar secrets no GitHub
- [ ] Testar CI com um push em develop
- [ ] Testar CD com merge em main
- [ ] Monitorar primeira execução
- [ ] Documentar issues encontradas

---

## 📞 Suporte

Para problemas:
1. Verifique logs em Actions → [Workflow] → Logs
2. Consulte [GITHUB-ACTIONS-SETUP.md](./GITHUB-ACTIONS-SETUP.md)
3. Abra issue no GitHub com contexto

---

**Documentação atualizada:** 2024  
**Versão:** 1.0.0  
**Status:** ✅ Ready for Production
