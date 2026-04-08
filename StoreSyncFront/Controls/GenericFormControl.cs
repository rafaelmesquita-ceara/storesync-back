﻿using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace StoreSyncFront.Controls;

/// <summary>
/// Um controle de contêiner que exibe um cabeçalho e conteúdo dentro de um Card.
/// </summary>
public class GenericFormControl : TemplatedControl
{
    // 1. Propriedade para o texto do cabeçalho.
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<GenericFormControl, string?>(nameof(Header));

    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    // 2. Propriedade para o conteúdo principal, que pode ser qualquer controle.
    // O atributo [Content] nos permite colocar XAML diretamente dentro das tags do nosso controle.
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<GenericFormControl, object?>(nameof(Content));

    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
}