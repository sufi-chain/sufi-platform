# SbMarkdownViewer

Renders markdown content as HTML using the Markdig library. Supports advanced markdown extensions including tables, task lists, and code blocks.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Content | string? | null | The markdown content to render |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary<string, object>? | null | Additional HTML attributes |

## CSS Classes

- `sb-markdown-viewer` - Base class applied to the container

## Markdown Features

The component uses Markdig with advanced extensions enabled, supporting:

- **Standard Markdown**: Headings, paragraphs, lists, links, images, emphasis
- **Extended Syntax**: Tables, task lists, strikethrough
- **Code Blocks**: Fenced code blocks with language specification
- **Auto-links**: Automatic URL linking
- **And more**: Abbreviations, footnotes, etc.

## Examples

### Basic Usage

```razor
<SbMarkdownViewer Content="@markdownContent" />

@code {
    private string markdownContent = """
        # Hello World
        
        This is **bold** and this is *italic*.
        
        - Item 1
        - Item 2
        - Item 3
        """;
}
```

### From External Source

```razor
@inject HttpClient Http

<SbMarkdownViewer Content="@readmeContent" />

@code {
    private string? readmeContent;
    
    protected override async Task OnInitializedAsync()
    {
        readmeContent = await Http.GetStringAsync("README.md");
    }
}
```

### With Code Blocks

```razor
<SbMarkdownViewer Content="@codeExample" />

@code {
    private string codeExample = """
        ## Code Example
        
        Here's how to create a button:
        
        ```razor
        <SbButton OnClick="HandleClick">
            Click Me
        </SbButton>
        ```
        
        And the handler:
        
        ```csharp
        private void HandleClick()
        {
            Console.WriteLine("Clicked!");
        }
        ```
        """;
}
```

### With Tables

```razor
<SbMarkdownViewer Content="@tableContent" />

@code {
    private string tableContent = """
        ## Parameters
        
        | Parameter | Type | Default | Description |
        |-----------|------|---------|-------------|
        | Name | string | null | The component name |
        | Size | SbSize | Md | Component size |
        | Disabled | bool | false | Disables the component |
        """;
}
```

### With Task Lists

```razor
<SbMarkdownViewer Content="@taskList" />

@code {
    private string taskList = """
        ## TODO
        
        - [x] Create component
        - [x] Add parameters
        - [ ] Write documentation
        - [ ] Add tests
        """;
}
```

### Documentation Page

```razor
<SbCard>
    <Header>
        <SbHeading Level="2">SbButton Documentation</SbHeading>
    </Header>
    <ChildContent>
        <SbMarkdownViewer Content="@buttonDocs" Class="documentation-content" />
    </ChildContent>
</SbCard>

@code {
    private string buttonDocs = """
        A versatile button component for triggering actions.
        
        ## Basic Usage
        
        ```razor
        <SbButton>Click Me</SbButton>
        ```
        
        ## Variants
        
        - **Primary**: Default filled button
        - **Outlined**: Button with border
        - **Ghost**: Transparent button
        
        ## Props
        
        | Prop | Type | Description |
        |------|------|-------------|
        | Variant | SbButtonVariant | Visual style |
        | Size | SbSize | Button size |
        | Disabled | bool | Disables interaction |
        """;
}
```

### Styled Container

```razor
<SbMarkdownViewer 
    Content="@content" 
    Class="custom-markdown"
    Style="max-width: 800px; margin: 0 auto;" />

<style>
    .custom-markdown h1 {
        border-bottom: 2px solid var(--sb-primary);
        padding-bottom: 0.5rem;
    }
    
    .custom-markdown code {
        background-color: #f5f5f5;
        padding: 2px 6px;
        border-radius: 4px;
    }
    
    .custom-markdown pre {
        background-color: #1e1e1e;
        color: #d4d4d4;
        padding: 1rem;
        border-radius: 8px;
        overflow-x: auto;
    }
</style>
```

### Blog Post

```razor
<article>
    <SbHeading Level="1">@post.Title</SbHeading>
    <SbText Variant="SbTextVariant.Caption" Color="SbColor.Secondary">
        @post.Date.ToLongDateString() · @post.ReadTime min read
    </SbText>
    <SbDivider />
    <SbMarkdownViewer Content="@post.Body" />
</article>
```

### Component Demo Page

```razor
<SbTabs>
    <SbTab Title="Preview">
        @* Live component demo *@
        <SbButton Variant="SbButtonVariant.Primary">Demo Button</SbButton>
    </SbTab>
    <SbTab Title="Code">
        <SbMarkdownViewer Content="@codeSnippet" />
    </SbTab>
    <SbTab Title="API">
        <SbMarkdownViewer Content="@apiDocs" />
    </SbTab>
</SbTabs>

@code {
    private string codeSnippet = """
        ```razor
        <SbButton Variant="SbButtonVariant.Primary">
            Demo Button
        </SbButton>
        ```
        """;
    
    private string apiDocs = """
        ## Parameters
        
        | Parameter | Type | Default |
        |-----------|------|---------|
        | Variant | SbButtonVariant | Primary |
        | Size | SbSize | Md |
        """;
}
```
