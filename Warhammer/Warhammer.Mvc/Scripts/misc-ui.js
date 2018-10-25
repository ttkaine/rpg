function showEditDate(panelId)
{
    $("#" + panelId + " .DisplayDatePanel").slideUp(400);
    $("#" + panelId + " .EditDatePanel").slideDown(400);
    $("#" + panelId + " .ShowEditDateButton").css("display", "none");
    $("#" + panelId + " .SubmitDateButton").css("display", "block");
}