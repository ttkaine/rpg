function showEditDate(panelId)
{
    $("#" + panelId + " .DisplayDatePanel").slideUp(400);
    $("#" + panelId + " .EditDatePanel").slideDown(400);
    $("#" + panelId + " .ShowEditDateButton").css("display", "none");
    $("#" + panelId + " .SubmitDateButton").css("display", "block");
}

function confirmAddSession()
{
    if ($("#selectArcSession option:selected").attr("data-is-in-arc") == "True")
    {
        var confirmText = '"' + $("#selectArcSession option:selected").text().trim() +
            '" is already added to arc "' +
            $("#selectArcSession option:selected").attr("data-current-arc-name").trim() +
            '".  Would you like to remove it from that arc and add it to the current arc?';

        return confirm(confirmText);
    }

    return true;
}

function showEditSessionArc()
{
    $("#sessionArcDisplayPanel").slideUp(400);
    $("#sessionArcEditPanel").slideDown(400);
    $("#btnShowEditSessionArc").css("display", "none");
    $("#btnUpdateSessionArc").css("display", "block");
}