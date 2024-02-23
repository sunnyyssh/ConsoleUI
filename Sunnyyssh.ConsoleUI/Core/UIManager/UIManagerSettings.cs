using System.Runtime.Versioning;

namespace Sunnyyssh.ConsoleUI;

public sealed class UIManagerSettings
{
    private readonly Color _defaultBackground = Color.Black;
    private readonly Color _defaultForeground = Color.White;
    public Color DefaultForeground
    {
        get => _defaultForeground;
        init
        {
            if (value == Color.Transparent || value == Color.Default)
            {
                return;
            }

            _defaultForeground = value;
        }
    }
    public Color DefaultBackground
    {
        get => _defaultBackground;
        init
        {
            if (value == Color.Transparent || value == Color.Default)
            {
                return;
            }

            _defaultBackground = value;
        }
    }
    
    public int? Height { get; [SupportedOSPlatform("Windows")] init; } = null;
    
    public int? Width { get; [SupportedOSPlatform("Windows")] init; } = null;

    public bool BorderConflictsAllowed { get; init; } = true;

    public ConsoleKey[] FocusChangeKeys { get; init; } = new[] { ConsoleKey.Tab };

    // Now there is no need in overlapping disabled.
    public bool EnableOverlapping { get; private init; } = true;

    public ConsoleKey? KillUIKey { get; init; } = null;
}

/*
Что должен предоставлять UIManager разработчику:
1. События начала, завершения работы интерфейса
2. Возможность добавлять элементы в любую точку по абсолютной или относительной координате
    - Нужно решить, что решить в какую сторону и как округлять относительные размеры при переводе в абсолютный
    - Можно ли добавлять элементы во время исполнения?
    - Может нужно дать возможность делать разметку в xml
3. Возможность удалять элементы
4. Настраивать, как должны происходить конфликты за территорию
    - Вариант выбрасывать исключения 
    - Перекрывать друг друга по приоритету (Предпочтительно)
5. Настраивать, как должна обрабатываться попытка нарисовать за пределами буфера
6. Настраивать default background и foreground
7. Настраивать поток исключений (По возможности)

Что должен предоставлять UIManager элементу UIElement:
1. Возможность перерисовать
2. Возможность получить или отдать фокус
3. 
*/