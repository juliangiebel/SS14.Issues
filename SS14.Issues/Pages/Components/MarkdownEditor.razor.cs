using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Serilog;

namespace SS14.Issues.Pages.Components;

    

public partial class MarkdownEditor : InputBase<string?>
{
    private InputTextArea _markdownInput;
    
    private bool _showPreview = false;

    private MarkdownPipeline _pipeline;
    
    private MarkupString _renderedPreview;
    
    private int[]? _lastSelection;

    private delegate string MarkdownTemplate(string selection);

    public MarkdownEditor()
    {
        _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    }

    private static string HeadingTemplate(string selection) => $"\n# {selection}";
    private static string BoldTemplate(string selection) => $"**{selection}**";
    private static string ItalicTemplate(string selection) => $"_{selection}_";
    private static string QuoteTemplate(string selection) => $"\n> {selection}";
    private static string CodeTemplate(string selection) => $"\n```\n{selection}\n```";
    private static string LinkTemplate(string selection) => $"[{selection}]()";
    private static string UnorderedListTemplate(string selection) => $"\n- {selection}";
    private static string OrderedListTemplate(string selection) => $"\n1. {selection}";
    private static string TasksTemplate(string selection) => $"\n- [ ] {selection}";
    
    private void OnWriteButtonClick()
    {
        _showPreview = false;
        StateHasChanged();
    }

    private void OnPreviewButtonClick()
    {
        _showPreview = true;
        _renderedPreview = new MarkupString(Markdown.ToHtml(CurrentValue ?? "", _pipeline));
        StateHasChanged();
    }

    private async Task InsertTemplate(MarkdownTemplate template)
    {
        var text = CurrentValue ?? "";
        
        if (_lastSelection == null || _lastSelection.Length < 2 || _lastSelection[0] == _lastSelection[1])
        {
            var cursorPosition = await JsRuntime.InvokeAsync<int>("getCursorPosition", _markdownInput.Element);
            CurrentValue = text.Insert(cursorPosition, template.Invoke(""));
            StateHasChanged();
            return;
        }

        var selectionStart = _lastSelection[0];
        var length = _lastSelection[1] - selectionStart;
        
        var selectedText = text.Substring(selectionStart, length);
        text = text.Remove(selectionStart, length);
        CurrentValue = text.Insert(selectionStart, template.Invoke(selectedText));
        StateHasChanged();
    }
    
    private void OnValueChange(string? text)
    {
        CurrentValue = text;
    }

    private void OnInput(ChangeEventArgs args)
    {
        
    }

    
    
    private async Task OnKeyUp()
    {
        _lastSelection = await JsRuntime.InvokeAsync<int[]>("getSelectedText", _markdownInput.Element);
    }
    
    private async Task OnMouseUp()
    {
        _lastSelection = await JsRuntime.InvokeAsync<int[]>("getSelectedText", _markdownInput.Element);
    }

    protected override bool TryParseValueFromString(string? value, out string? result, out string? validationErrorMessage)
    {
        result = value;
        validationErrorMessage = null;
        return true;
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("populateOcticons");
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to populate octicons");
        }
    }
}