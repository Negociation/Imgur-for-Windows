# Auditoria de compatibilidade e crash

**Repositório:** Giuraffe - Imgur for Windows  
**Base:** `0fa5edf` — LifeCycle  
**Data da auditoria:** 2025-02-14  
**Escopo:** alterações não commitadas desde o trabalho de Settings até as funcionalidades de notificações, ciclo de vida, SQLite, startup e ajustes visuais.

## Resumo executivo

A solução compila sem erros e o `git diff --check` não encontrou problemas de whitespace. Porém, existem riscos de compatibilidade em tempo de execução e uma possível origem para o erro XAML/WinRT:

```text
0x80070057 - Parâmetro incorreto
winrt::hresult_invalid_argument
System.ArgumentException
```

O `KernelBase.dll` e o `Windows.UI.Xaml.dll` aparecem no final da cadeia porque propagam a exceção não tratada. Eles não identificam necessariamente a origem do problema.

## Validações executadas

- Build da solução: sem erros de compilação.
- `git diff --check`: sem erros.
- Projetos compartilhados: compilam para `netstandard1.4`.
- Não existe projeto configurado para `netstandard1.1`.
- O log detalhado foi adicionado em `Imgur.Uwp/App.xaml.cs`.
- O log persistido é `unhandled-errors.log`, dentro de `ApplicationData.Current.LocalFolder`.

## Erro observado

Trechos relevantes do log:

```text
WinRT originate error - 0x80070057: 'Parâmetro incorreto.'
winrt::hresult_invalid_argument
[App.UnhandledException] System.ArgumentException: Parâmetro incorreto.
Exceção sem tratamento em Windows.UI.Xaml.dll: 0xC000027B
```

Também aparecem erros `0x80070005 - Acesso negado`, relacionados a cache de shaders/DirectX, armazenamento UWP e conectividade. Esses erros parecem secundários em relação ao `0x80070057`.

## Áreas prioritárias do crash

A ordem recomendada para isolar o problema é:

1. `Imgur.Uwp/Views/Shell/ShellView.xaml`
2. `Imgur.Uwp/Views/Shell/ShellView.xaml.cs`
3. `Imgur.Uwp/Resources/ControlStylesDictionary.xaml`
4. `Imgur.Uwp/Controls/NavigationViewItem.xaml`
5. `Imgur/ViewModels/Shell/ShellViewModel.cs`
6. `Imgur.Uwp/Dialogs/NotificationsDialog.xaml`
7. `Imgur.Uwp/Services/AccountInboxService.cs`
8. `Imgur/ViewModels/Notifications/NotificationsDialogViewModel.cs`

### Alterações sensíveis no caminho do crash

- Template customizado de `NavigationView`.
- Inclusão de `HamburgerUnreadBadge` dentro do template.
- Busca recursiva de elementos com `VisualTreeHelper`.
- Novo evento `NavigationView_PaneClosed`.
- Novo `NavigationViewItem` de notificações.
- Inicialização assíncrona de notificações no `ShellViewModel`.
- Operações fire-and-forget com `_ = ...Async()`.
- Abertura de `NotificationsDialog` a partir do shell.
- Alteração de estilos de controles virtualizados.

## Incompatibilidade com Creators Update

O projeto declara:

```xml
<TargetPlatformMinVersion>10.0.15063.0</TargetPlatformMinVersion>
```

Entretanto, também utiliza:

```xml
<PackageReference Include="Microsoft.UI.Xaml">
  <Version>2.7.3</Version>
</PackageReference>
```

E o manifesto contém:

```xml
<uap5:Extension Category="windows.startupTask" ...>
```

Além disso, o próprio projeto possui comentário indicando que a combinação atual do SQLite exige no mínimo `16299`.

### Conclusão de compatibilidade

A configuração está inconsistente para suporte ao Creators Update `15063`. Deve ser escolhida uma estratégia:

- **Manter suporte ao Creators Update:** reduzir ou substituir APIs/pacotes que exigem versões posteriores.
- **Adotar o mínimo moderno:** elevar `TargetPlatformMinVersion` para uma versão compatível com WinUI, SQLite e `uap5:StartupTask`.

Não é seguro declarar `15063` como mínimo enquanto esses componentes permanecerem no projeto sem validação em uma máquina com Creators Update.

## Problema de segurança no log

Em `Imgur.Api/Services/Actions/ApiService.cs`, existe registro do token completo:

```csharp
Debug.WriteLine("Fazendo Request via AccessToken" + token);
```

Isso expõe o token OAuth no Output do Visual Studio. O token deve ser removido do log ou mascarado, por exemplo:

```text
Fazendo Request via AccessToken: ****abcd
```

## Operações assíncronas sem observação

Foram encontradas chamadas como:

```csharp
_ = _inbox.RefreshUnreadAsync();
_ = SetStartupLaunchAsync(value);
_ = RefreshStartupStateAsync();
```

Essas chamadas podem ocultar falhas. Pontos de atenção:

- `NotificationsDialogViewModel.InitializeAsync()` não possui `try/finally`.
- `GetInboxAsync()` pode propagar exceções para a camada XAML.
- `ShellView.OnViewModelPropertyChanged` é `async void` e executa `Task.Delay(1000)`.
- Uma exceção após a página ser desmontada pode ser encaminhada pelo `Windows.UI.Xaml.dll` como erro não tratado.

## Logger adicionado

O método `LogException` em `Imgur.Uwp/App.xaml.cs` registra:

- Fonte da exceção.
- Mensagem.
- Tipo completo.
- HRESULT.
- Stack trace.
- Todas as `InnerException`.
- Cópia no Output do Visual Studio.
- Cópia em `unhandled-errors.log`.

Não marcar `e.Handled = true` automaticamente, pois isso esconderia a falha real.

## Procedimento para reproduzir e investigar

1. Executar o app com o depurador anexado.
2. Reproduzir a abertura do menu de notificações.
3. Abrir o arquivo `unhandled-errors.log` na pasta `LocalState` do pacote UWP.
4. Procurar por:

```text
HRESULT: 0x80070057
```

5. Examinar o `StackTrace` imediatamente abaixo.
6. Comparar se a exceção ocorre antes ou depois da criação do `NotificationsDialog`.
7. Repetir o teste com o badge do hamburger temporariamente removido.
8. Repetir o teste com o template customizado de `NavigationView` temporariamente desativado.

## Checklist de isolamento

- [ ] Remover temporariamente `HamburgerUnreadBadge`.
- [ ] Remover temporariamente o estilo customizado de `NavigationView`.
- [ ] Desabilitar `RefreshUnreadAsync()` no `ShellViewModel`.
- [ ] Abrir o `NotificationsDialog` sem carregar dados da API.
- [ ] Confirmar se o crash ocorre somente com usuário autenticado.
- [ ] Testar com token válido e token expirado.
- [ ] Remover o token dos logs.
- [ ] Testar na versão mínima declarada do Windows.
- [ ] Definir uma versão mínima realista para WinUI/SQLite/startup task.

## Conclusão

A alteração está compilando, mas ainda não está validada como compatível com o Creators Update. O maior risco técnico é a combinação de `TargetPlatformMinVersion 15063` com `Microsoft.UI.Xaml 2.7.3`, `uap5:StartupTask` e a versão atual do SQLite.

Para o crash atual, a investigação deve começar pelo caminho de notificações e pelo template customizado do `NavigationView`, usando o `unhandled-errors.log` para obter o método exato que gera o `ArgumentException`.
