Execute o fluxo de trabalho TDD completo para finalizar a feature que acabou de ser implementada. Siga exatamente os passos abaixo:

## Contexto

Este projeto é um ASP.NET Core 9.0 com TDD obrigatório. O runtime correto é .NET 9:
- `export DOTNET_ROOT="/opt/homebrew/opt/dotnet@9/libexec"`
- `export PATH="/opt/homebrew/opt/dotnet@9/bin:$PATH"`

Projeto de testes: `StoreSyncBack.Tests/StoreSyncBack.Tests.csproj`

## Passos obrigatórios

### 1. Identificar o estado atual
- Verifique o branch atual com `git branch`
- Verifique o que foi alterado com `git status` e `git diff`
- Se já estiver em um branch de feature, continue nele. Se estiver em `main`, crie um novo branch com nome descritivo: `feature/<nome-curto-da-feature>`

### 2. Executar os testes e confirmar que passam (verde)
```bash
export DOTNET_ROOT="/opt/homebrew/opt/dotnet@9/libexec"
export PATH="/opt/homebrew/opt/dotnet@9/bin:$PATH"
dotnet test StoreSyncBack.Tests/StoreSyncBack.Tests.csproj --logger "console;verbosity=normal"
```
- Se algum teste falhar, corrija antes de prosseguir

### 3. Commit com mensagem objetiva
- Faça o commit de **todos** os arquivos alterados (implementação + testes)
- Mensagem no formato: `tipo: resumo objetivo do que foi feito`
- Tipos: `feat`, `fix`, `refactor`, `test`, `docs`
- Inclua o trailer: `Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>`

### 4. Push do branch
```bash
git push -u origin <nome-do-branch>
```

### 5. Merge no main
```bash
git checkout main
git merge <nome-do-branch> --no-ff -m "merge: <nome-do-branch> -> main"
git push origin main
```

### 6. Reportar o resultado
Ao final, informe:
- Branch criado
- Quantos testes existem e quantos passaram
- Resumo do commit
- Confirmação do merge no main
