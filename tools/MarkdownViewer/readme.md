# Markdown Viewer - Professional .NET WPF Application

A professional-grade markdown viewer built with WPF, Material Design, and SOLID architecture principles.

## 🎨 Features

### User Interface
- **Material Design UI** - Modern, sleek interface using MaterialDesignInXAML
- **Dark/Light Themes** - Toggle between themes with one click
- **Responsive Layout** - Resizable panels with splitter
- **File Browser** - Hierarchical file list with search
- **Table of Contents** - Auto-generated navigation tree
- **WebView2 Rendering** - High-fidelity markdown rendering

### Functionality
- **Real-time Search** - Filter files by name or path
- **Export to HTML** - Save rendered documents
- **Print Support** - Print directly from viewer
- **File Metadata** - View size, modification date
- **Keyboard Shortcuts** - F5 refresh, Ctrl+O open folder

## 🏗️ Architecture

This application follows **SOLID principles** and clean architecture:

### Single Responsibility Principle
Each class has one reason to change:
- `MarkdownService` - Converts markdown to HTML
- `TemplateService` - Loads and renders templates
- `StyleService` - Manages CSS styles
- `DocumentRenderer` - Orchestrates document creation
- `MainWindow` - Handles UI interactions

### Open/Closed Principle
The system is open for extension, closed for modification:
- New themes can be added by creating CSS files
- New templates can be added without changing code
- Rendering pipeline is extensible

### Dependency Inversion Principle
High-level modules depend on abstractions:
```csharp
public interface IMarkdownService { }
public interface ITemplateService { }
public interface IStyleService { }
public interface IDocumentRenderer { }
```

### Project Structure
```
MarkdownViewer/
├── Services/              # Business logic layer
│   ├── IMarkdownService.cs
│   ├── MarkdownService.cs
│   ├── ITemplateService.cs
│   ├── TemplateService.cs
│   ├── IStyleService.cs
│   ├── StyleService.cs
│   ├── IDocumentRenderer.cs
│   └── DocumentRenderer.cs
├── Templates/             # HTML templates (data-driven)
│   └── document.html
├── Styles/                # CSS themes (data-driven)
│   ├── light-theme.css
│   └── dark-theme.css
├── MainWindow.xaml        # UI definition
├── MainWindow.xaml.cs     # Presentation layer
└── App.xaml               # Application entry point
```

## 🚀 Getting Started

### Prerequisites
- .NET Framework 4.8
- WebView2 Runtime (usually pre-installed on Windows 10/11)

### Building
```powershell
cd Tools/MarkdownViewer
dotnet restore
dotnet build -c Release
```

### Running
```powershell
.\bin\Release\net48\MarkdownViewer.exe
```

Or double-click the executable in Windows Explorer.

## 📖 Usage

### Opening Files
1. Click **Open Folder** button
2. Select a folder containing markdown files
3. Files appear in the left panel
4. Click any file to view it

### Searching
1. Type in the search box
2. Files filter in real-time
3. Click **X** to clear search

### Changing Themes
1. Click the **Theme** button (moon/sun icon)
2. Theme applies to both UI and markdown content

### Table of Contents
1. Click **TOC** button to show/hide
2. Click any heading to navigate (planned feature)

### Exporting
1. Open a markdown file
2. Click **Export** button
3. Choose save location
4. HTML file includes embedded styles

### Printing
1. Open a markdown file
2. Click **Print** button
3. Use browser print dialog

## 🎨 Customization

### Adding New Themes
1. Create a new CSS file in `Styles/` folder
2. Name it `your-theme-name.css`
3. Follow the existing structure
4. Theme will be automatically available

### Adding New Templates
1. Create HTML file in `Templates/` folder
2. Use `{{PLACEHOLDER}}` syntax for substitution
3. Load via `ITemplateService`

### Example Theme Structure
```css
/* Styles/custom-theme.css */
* { margin: 0; padding: 0; box-sizing: border-box; }

body {
    font-family: 'Your Font', sans-serif;
    color: #your-text-color;
    background-color: #your-bg-color;
}

h1 { color: #your-heading-color; }
/* ... more styles ... */
```

## 🔧 Technical Details

### Dependencies
- **Markdig** (0.33.0) - Markdown processing
- **MaterialDesignThemes** (4.9.0) - UI components
- **MaterialDesignColors** (2.1.4) - Color system
- **Microsoft.Web.WebView2** (1.0.2088.41) - Rendering engine
- **Newtonsoft.Json** (13.0.3) - JSON handling

### Key Technologies
- **WPF** - Windows Presentation Foundation
- **XAML** - UI markup
- **C# 7.3** - Programming language
- **.NET Framework 4.8** - Runtime

### Design Patterns
- **Dependency Injection** - Manual DI in constructor
- **Service Layer Pattern** - Business logic separation
- **Repository Pattern** - File system access
- **Template Method Pattern** - Document rendering
- **Strategy Pattern** - Theme switching

## 📊 Supported Markdown

- Headers (H1-H6) with anchor links
- **Bold**, *italic*, ~~strikethrough~~
- `Inline code` and code blocks
- Tables with headers
- Ordered and unordered lists
- [Links](https://example.com) and images
- > Blockquotes
- Horizontal rules
- Emojis :smile:

## 🐛 Troubleshooting

### WebView2 Not Found
Install WebView2 Runtime from:
https://developer.microsoft.com/microsoft-edge/webview2/

### Files Not Showing
Ensure the folder contains `.md` files and you have read permissions.

### Theme Not Applying
Check that CSS file exists in `Styles/` folder and follows naming convention.

## 📝 Future Enhancements

- [ ] Syntax highlighting for code blocks
- [ ] Live markdown editing
- [ ] Git integration for versioned docs
- [ ] Full-text search across all files
- [ ] Recent files list
- [ ] Favorites/bookmarks
- [ ] PDF export
- [ ] Markdown editor mode

## 🤝 Contributing

This application demonstrates professional .NET development practices:
- Clean code
- SOLID principles
- Separation of concerns
- Data-driven design
- Testable architecture

## 📄 License

Part of the Canonical project.

## 👏 Acknowledgments

- **Material Design in XAML** - Beautiful UI components
- **Markdig** - Excellent markdown processing
- **Microsoft WebView2** - Modern web rendering

---

**Built with professional standards. No hard-coded strings in business logic. Data-driven. SOLID.**