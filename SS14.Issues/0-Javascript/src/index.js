let octicons = require("@primer/octicons");

window.populateOcticons = function()
{
    console.log("Populating octicons")
    let elements = document.getElementsByClassName('icon');
    for (const element of elements) {
        let name = element.getAttribute('data-icon');
        if (!name)
            throw new Error("Used icon without data-icon attribute on element: " + element.toString())

        try {
            element.innerHTML = octicons[name].toSVG();
        }
        catch (e)
        {
            console.log(e);
        }
    }
}


window.getCursorPosition = (textarea) => textarea.selectionStart;

window.getSelectedText = (textarea) => [textarea.selectionStart, textarea.selectionEnd];