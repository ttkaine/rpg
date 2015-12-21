
var lastPostId;
var lastUpdateTime;
var refreshInterval;
var selectedTab;
var recipients;
var sessionId;
var rootPath;
var initialPostComplete;
var isMyTurn;

function setupPage(id)
{
    rootPath = getAjaxWebService();

    isMyTurn = false;

    sessionId = id;
    lastPostId = 0;
    lastUpdateTime = "01 Jan 2000 00:00:00";
    initialPostComplete = false;
    selectedTab = 1;
    recipients = new Array();

    updatePlayerToPost();

    setupDiceDropDowns();
    setSessionTitle();
    setupCharacterDropDown();
    addNewPosts(false);
    toggleOoc(false);
    setupCharacterDetails();    

    $("#divOverlay").attr("style", "display:none;");
    refreshInterval = setInterval(pageRefresh, 3000);

    $(window).focus(function()
    {
        document.title = document.title.replace("* ", "");
    });

    checkNotificationPermission();
}

function checkNotificationPermission()
{
    if ("Notification" in window)
    {
        if (Notification.permission != "granted" && Notification.permission != "denied")
        {
            Notification.requestPermission();
        }
    }
}

function setupDiceDropDowns()
{
    $("#ddlDiceCount").empty();
    for (var i = 1; i <= 30; i++)
    {
        $("#ddlDiceCount").append($("<option></option>").text(i).val(i));
    }

}

function selectedTabChanged(tab)
{
    selectedTab = tab;

    if (selectedTab == 1)
    {
        $("#divInCharacterButton").attr("class", "ToggleButtonEnabled");
        $("#divInCharacterButton").attr("checked", "checked");
        $("#divPlayerPostControls").attr("style", "background-color:#fff;");
        $("#divCharacterControls").attr("style", "background-color:#fff;");
        $("#imgCharacter").attr("style", "display:block;");
        $("#btnEditCharacter").attr("style", "display:block;");
    }
    else
    {
        $("#divInCharacterButton").attr("class", "ToggleButtonDisabled");
        $("#divInCharacterButton").attr("checked", "unchecked");
        $("#divCharacterControls").attr("style", "background-color:#eaea99;");
        $("#imgCharacter").attr("style", "display:none;");
        $("#btnEditCharacter").attr("style", "display:none;");
    }

    if (selectedTab == 2)
    {
        $("#divPlayerPostControls").attr("style", "background-color:#ffffaa;");
        $("#divOutOfCharacterButton").attr("class", "ToggleButtonEnabled");
        $("#divOutOfCharacterButton").attr("checked", "checked");
    }
    else
    {
        $("#divOutOfCharacterButton").attr("class", "ToggleButtonDisabled");
        $("#divOutOfCharacterButton").attr("checked", "unchecked");
    }

    if (selectedTab == 3)
    {
        $("#divDiceRollButton").attr("class", "ToggleButtonEnabled");
        $("#divDiceRollButton").attr("checked", "checked");
        $("#divPlayerDiceControls").attr("style", "display:block;");
        $(".PlayerTextPostControls").attr("style", "display:none;");
    }
    else
    {
        $("#divDiceRollButton").attr("class", "ToggleButtonDisabled");
        $("#divDiceRollButton").attr("checked", "unchecked");
        $("#divPlayerDiceControls").attr("style", "display:none;");
        $(".PlayerTextPostControls").attr("style", "display:block;");
    }

    if (selectedTab == 1 || selectedTab == 3)
    {
        if ($("#ddlPostAs option").size() > 0)
        {
            $("#ddlPostAs").attr("style", "display:block;");
            $("#spanPostAs").html("Post As: ");
            $("#btnPost").removeAttr("disabled");
        }
        else
        {
            $("#ddlPostAs").attr("style", "display:none;");
            $("#spanPostAs").html("No characters available");
            $("#btnPost").attr("disabled", "disabled");
        }
    }
    else
    {
        $("#ddlPostAs").attr("style", "display:none;");
        $("#btnPost").removeAttr("disabled");
        $("#spanPostAs").html("Post As:&nbsp;&nbsp;OOC");
    }


}

function toggleOoc(doSlide)
{
    var outerHeight = $("#divPostContainer").outerHeight();
    var scrollTop = $("#divPostContainer").scrollTop();
    var scrollHeight = $("#divPostContainer").prop("scrollHeight");
    var scrollToEnd = (scrollHeight - scrollTop == outerHeight);

    var showOoc = $("#chkShowOoc").is(":checked");
    if (showOoc)
    {
        if (doSlide)
        {
            $(".OutOfCharacterPost").parent().slideDown(800);
        }
        else
        {
            $(".OutOfCharacterPost").parent().attr("stlye", "display:block;");
        }
    }
    else
    {
        if (doSlide)
        {
            $(".OutOfCharacterPost").parent().slideUp(800);
        }
        else
        {
            $(".OutOfCharacterPost").parent().attr("stlye", "display:none;");
        }
    }

    if (scrollToEnd)
    {
        scrollHeight = $("#divPostContainer").prop("scrollHeight");
        $("#divPostContainer").scrollTop(scrollHeight);
    }
}

//function queryString(key)
//{
//    var re = new RegExp('(?:\\?|&)' + key + '=(.*?)(?=&|$)', 'gi');
//    var r = [], m;
//    while ((m = re.exec(document.location.search)) != null) r.push(m[1]);
//    return r;
//}

function setSessionTitle()
{
    //var sessionId = queryString("id");
    var parameters = '{"sessionId": ' + sessionId + ' }';

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: rootPath + "/GetSessionTitle",
        data: parameters,
        dataType: "json",
        async: false,
        success: function (data)
        {
            var jsonData = eval(data);
            $("#divSessionTitle").html(jsonData);
        },
        error: function (jqXHR, textStatus, errorThrown)
        {
            $("#divSessionTitle").html("Session Title");
        }
    });
}

function generateNotification(message)
{
    if (initialPostComplete && !document.hasFocus())
    {
        if (document.title.substring(0, 2) != "* ")
        {
            document.title = "* " + document.title;
        }

        if ("Notification" in window)
        {
            if (Notification.permission === "granted")
            {
                var title = $("#divSessionTitle").html();
                var options = {
                    //title: $("#divSessionTitle").html(),
                    body: message,
                    icon: "/content/images/roleplayforum/notify-icon.jpg"
                };

                var notification = new Notification(title, options);
                setTimeout(notification.close.bind(notification), 10000);
            }
        }
    }
}

function handleNewPosts(jsonData, scrollToEnd)
{
    if (!jsonData.IsError)
    {
        if (jsonData.Count > 0)
        {
            var doSequentialFadeIn = (lastPostId > 0);
            if (lastPostId < jsonData.LatestPostId)
            {
                lastPostId = jsonData.LatestPostId;
            }
            var itemsAdded = 0;
            for (var i = 0; i < jsonData.Posts.length; i++)
            {
                if ($("#" + jsonData.Posts[i].ID).length == 0)
                {
                    $("#divPostContainer").append(jsonData.Posts[i].Content);
                    if (doSequentialFadeIn)
                    {
                        $("#cover" + jsonData.Posts[i].ID).delay(itemsAdded * 1000).fadeOut(1000);
                    }
                    else
                    {
                        $("#cover" + jsonData.Posts[i].ID).attr("style", "display:none;");
                    }
                    itemsAdded++;
                }
            }

            generateNotification("New posts have been added to the session.");
        }

        if (jsonData.EditedCount > 0)
        {
            for (var i = 0; i < jsonData.EditedPosts.length; i++)
            {
                $("#" + jsonData.EditedPosts[i].ID).parent().replaceWith(jsonData.EditedPosts[i].Content);
                $("#cover" + jsonData.EditedPosts[i].ID).attr("style", "display:none;");
            }

            generateNotification("One or more posts have been edited.");
        }

        if (jsonData.DeletedCount > 0)
        {
            for (var i = 0; i < jsonData.DeletedPosts.length; i++)
            {
                if ($("#post" + jsonData.DeletedPosts[i]).length > 0)
                {
                    $("#post" + jsonData.DeletedPosts[i]).parent().slideUp(800, function ()
                    {
                        $("#post" + jsonData.DeletedPosts[i]).parent().remove();
                    });
                }
            }

            generateNotification("One or more posts have been deleted.");
        }

        lastUpdateTime = jsonData.LastUpdate;

        if (scrollToEnd)
        {
            scrollHeight = $("#divPostContainer").prop("scrollHeight");
            $("#divPostContainer").scrollTop(scrollHeight);
        }
    }
    else
    {
        alert(jsonData.ErrorMessage);
    }

    initialPostComplete = true;
}

function addNewPosts(doAsync)
{
    //var sessionId = queryString("id");
    var parameters = '{"sessionId": ' + sessionId + ', "lastPostId": ' + lastPostId + ', "lastUpdateTime": "' + lastUpdateTime + '" }';
    var outerHeight = $("#divPostContainer").outerHeight();
    var scrollTop = $("#divPostContainer").scrollTop();
    var scrollHeight = $("#divPostContainer").prop("scrollHeight");
    var scrollToEnd = (scrollHeight - scrollTop == outerHeight);

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: rootPath + "/GetLatestPostsForSession",
        data: parameters,
        dataType: "json",
        async: doAsync,
        success: function (data)
        {
            var jsonData = eval(data)[0];
            handleNewPosts(jsonData, scrollToEnd);
        },
        error: function (jqXHR, textStatus, errorThrown)
        {
            if (lastPostId == 0)
            {
                alert("Session timeout.");
                window.location.replace("/");
            }
        }
    });
}

function pageRefresh()
{
    addNewPosts(true);
    toggleOoc(false);
    updatePlayerToPost();
}

function setupCharacterDropDown()
{
    //var sessionId = queryString("id");
    var parameters = '{"sessionId": ' + sessionId + ' }';

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: rootPath + "/GetCharacterList",
        data: parameters,
        dataType: "json",
        async: false,
        success: function (data)
        {
            var jsonData = eval(data);
            $("#ddlPostAs").empty();
            if (jsonData.length > 0)
            {
                $.each(jsonData, function (index, character)
                {
                    $("#ddlPostAs").append($("<option></option>").text(character.Name).val(character.ID));
                });
                $("#spanPostAs").html("Post As: ");
                $("#ddlPostAs").attr("style", "display:block;");
                $("#btnPost").removeAttr("disabled");
                $("#btnRoll").removeAttr("disabled");
            }
            else
            {
                $("#ddlPostAs").attr("style", "display:none;");
                $("#spanPostAs").html("No characters available");
                $("#btnPost").attr("disabled", "disabled");
                $("#btnRoll").attr("disabled", "disabled");
            }
        },
        error: function (jqXHR, textStatus, errorThrown)
        {
            alert("Session timeout.");
            window.location.replace("/");
        }
    });


}

function btnPost_Click()
{
    var text = $("#txtPost").val().trim();
    //if (text.length > 0)
    //{
        //$("#txtPost").val("");
        postSubmitted(text);
    //}
}

function txtPost_keyPress(event)
{
    /*if (event != null)
    {
        if (event.type == 'keypress' && event.keyCode == 13 && !event.shiftKey)
        {
            var text = $("#txtPost").val().trim();
            if (text.length > 0)
            {
                $("#txtPost").val("");
                postSubmitted(text);
            }

            event.preventDefault();
        }
    }*/

    updateCurrentPlayerTurn();
}

function postSubmitted(text)
{
    clearInterval(refreshInterval);

    var cleanedText = text.replace(/"/g, '&quote;');

    var isOoc = $("#divOutOfCharacterButton").attr("class") == "ToggleButtonEnabled";
    var characterId = -1;
    if ($("#ddlPostAs option").size() > 0)
    {
        characterId = $("#ddlPostAs").val();
    }
    var recipientString = "";
    //if (recipients.length > 0)
    //{
    //    recipientString = recipients.join(",");
    //}
    
    var parameters = '{"sessionId": ' + sessionId + ', "characterId": ' + characterId + ', "lastPostId": ' + lastPostId + ', "isOoc": ' + isOoc + ', "text": "' + cleanedText + '", "lastUpdateTime": "' + lastUpdateTime + '", "recipientString": "' + recipientString + '" }';
    var outerHeight = $("#divPostContainer").outerHeight();
    var scrollTop = $("#divPostContainer").scrollTop();
    var scrollHeight = $("#divPostContainer").prop("scrollHeight");
    //var scrollToEnd = (scrollHeight - scrollTop == outerHeight);
    var scrollToEnd = true;

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: rootPath + "/MakeTextPost",
        data: parameters,
        dataType: "json",
        async: true,
        success: function (data)
        {
            $("#txtPost").val("");
            $("#chkDeviceToggle").prop("checked", false);
            var jsonData = eval(data)[0];
            handleNewPosts(jsonData, scrollToEnd);
            $("#playerToPost").html(jsonData.PlayerTurnMessage);
            isMyTurn = jsonData.IsCurrentPlayerTurn;
            updateCurrentPlayerTurn();
            refreshInterval = setInterval(pageRefresh, 3000);
        },
        error: function (jqXHR, textStatus, errorThrown)
        {
            alert("Unable to post at this time.");
            refreshInterval = setInterval(pageRefresh, 3000);
        }
    });

}

function updateCurrentPlayerTurn()
{
    var text = $("#txtPost").val().trim();
    if (isMyTurn)
    {
        if (text.length > 0)
        {
            $("#btnPost").val("POST");
        }
        else
        {
            $("#btnPost").val("SKIP");
        }
        $("#playerToPost").css("background", "#006600");
    }
    else
    {
        //if (text.length > 0)
        //{
            $("#btnPost").val("POST OUT OF TURN");
        //}
        //else
        //{
        //    $("#btnPost").val("SKIP");
        //}
        $("#playerToPost").css("background", "#bb3333");
    }
}

function updatePlayerToPost()
{
    var parameters = '{"sessionId": ' + sessionId + ' }';

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: rootPath + "/GetCurrentPlayerToPost",
        data: parameters,
        dataType: "json",
        async: true,
        success: function (data)
        {
            var jsonData = eval(data)[0];

            $("#playerToPost").html(jsonData.Message);
            isMyTurn = jsonData.IsCurrentPlayer;
            updateCurrentPlayerTurn();
        },
        error: function (jqXHR, textStatus, errorThrown)
        {
            $("#playerToPost").css("background", "#ff6666");
            $("#playerToPost").html("Loading post order data...");
            updateCurrentPlayerTurn();
        }
    });
}

function ddlRollType_Change()
{
    var dicePool = $("#ddlRollType").val() == "1";

    if (dicePool)
    {
        $("#ddlDieSize").attr("style", "display:none;");
        $("#spanD10").attr("style", "display:block;");
        $("#divRollTarget").attr("style", "display:block;");
    }
    else
    {
        $("#ddlDieSize").attr("style", "display:block;");
        $("#spanD10").attr("style", "display:none;");
        $("#divRollTarget").attr("style", "display:none;");
    }
}

function btnRollDice_Click()
{
    postDiceRoll();
}

function postDiceRoll()
{
    clearInterval(refreshInterval);

    //var sessionId = queryString("id");
    var characterId = -1;
    if ($("#ddlPostAs option").size() > 0)
    {
        characterId = $("#ddlPostAs").val();
    }
    var rollType = $("#ddlRollType").val();
    var diceCount = $("#ddlDiceCount").val();
    var dieSize = 10;
    var rollTarget = 0;
    if (rollType == "2")
    {
        dieSize = $("#ddlDieSize").val();
    }
    else
    {
        rollTarget = $("#ddlRollTarget").val();
    }
    var reRollMaximums = $("#chkReRolls").is(':checked');

    var parameters = '{"sessionId": ' + sessionId + ', "characterId": ' + characterId + ', "lastPostId": ' + lastPostId + ', "dieSize": ' + dieSize + ', "dieCount": ' + diceCount + ', "rollType": ' + rollType + ', "rollTarget": ' + rollTarget + ', "reRollMaximum": ' + reRollMaximums + ', "lastUpdateTime": "' + lastUpdateTime + '" }';

    var outerHeight = $("#divPostContainer").outerHeight();
    var scrollTop = $("#divPostContainer").scrollTop();
    var scrollHeight = $("#divPostContainer").prop("scrollHeight");
    //var scrollToEnd = (scrollHeight - scrollTop == outerHeight);
    var scrollToEnd = true;

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: rootPath + "/MakeDiceRollPost",
        data: parameters,
        dataType: "json",
        async: true,
        success: function (data)
        {
            $("#chkDeviceToggle").prop("checked", false);
            var jsonData = eval(data)[0];
            handleNewPosts(jsonData, scrollToEnd);
            refreshInterval = setInterval(pageRefresh, 3000);
        },
        error: function (jqXHR, textStatus, errorThrown)
        {
            alert("Unable to post at this time.");
            refreshInterval = setInterval(pageRefresh, 3000);
        }
    });

}

//function viewCharacter(characterId, characterName)
//{
//    activateCharacterTab(1);

//    if ($("#CharacterPopOver").attr("shown") == "false")
//    {
//        $("#CharacterPopOver").animate({ "left": "5px", "opacity": "1" }, 1000);
//        $("#CharacterPopOver").attr("shown", "true");
//    }

//    $("#CharacterPopOverName").html(characterName);
//    $("#CharacterPopOverDescription").attr("style", "display:none;");
//    $("#CharacterPopOverLoading").attr("style", "display:block;");
//    //var sessionId = queryString("id");

//    var parameters = '{"sessionId": ' + sessionId + ', "characterId": ' + characterId + ' }';

//    $.ajax({
//        type: "POST",
//        contentType: "application/json; charset=utf-8",
//        url: rootPath + "/GetCharacterDetails",
//        data: parameters,
//        dataType: "json",
//        async: true,
//        success: function (data)
//        {
//            var jsonData = eval(data)[0];

//            $("#CharacterPopOverDescription").html('<img src="' + rootPath + '/image/' + characterId + '" class="CharacterPopOverImage" />' + jsonData.Description);
//            $("#CharacterPopOverSheet").html(jsonData.CharacterSheet);

//            $("#CharacterPopOverLoading").attr("style", "display:none;");
//            $("#CharacterPopOverDescription").attr("style", "display:block;");
//        },
//        error: function (jqXHR, textStatus, errorThrown)
//        {
//            $("#CharacterPopOverDescription").html("Unable to retrieve Character from server.");
//            $("#CharacterPopOverSheet").html("Unable to retrieve Character from server.");
//            $("#CharacterPopOverLoading").attr("style", "display:none;");
//            $("#CharacterPopOverDescription").attr("style", "display:block;");
//        }
//    });
//}

//function closeCharacterPopOver()
//{
//    if ($("#CharacterPopOver").attr("shown") == "true")
//    {
//        $("#CharacterPopOver").animate({ "left": "-650px", "opacity": "0" }, 1000, function ()
//        {
//            $("#CharacterPopOverSheet").html("");
//        });
//        $("#CharacterPopOver").attr("shown", "false");
//    }
//}

function deletePost(postId)
{
    var result = confirm("Are you sure you want to delete this post?");

    if (result == true)
    {
        clearInterval(refreshInterval);

        //var sessionId = queryString("id");
        var parameters = '{"sessionId": ' + sessionId + ', "postId": ' + postId + ', "lastPostId": ' + lastPostId + ', "lastUpdateTime": "' + lastUpdateTime + '" }';
        var outerHeight = $("#divPostContainer").outerHeight();
        var scrollTop = $("#divPostContainer").scrollTop();
        var scrollHeight = $("#divPostContainer").prop("scrollHeight");
        var scrollToEnd = (scrollHeight - scrollTop == outerHeight);

        $.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: rootPath + "/DeletePost",
            data: parameters,
            dataType: "json",
            async: true,
            success: function (data)
            {
                var jsonData = eval(data)[0];
                handleNewPosts(jsonData, scrollToEnd);
                refreshInterval = setInterval(pageRefresh, 3000);
            },
            error: function (jqXHR, textStatus, errorThrown)
            {
                alert("Unable to delete post at this time.");
                refreshInterval = setInterval(pageRefresh, 3000);
            }
        });       
    }
}

function editPost(postId)
{
    $("#btnEditPostSubmit").prop("disabled", false);
    $("#btnEditPostSubmit").attr("onclick", "btnEditPostSubmit_click(" + postId + ");");
    var text = $("#postContent" + postId).html();
    text = text.replace(/<br>/g, "\n");
    $("#txtEditPost").val(text);
    $("#modalOverlay").fadeIn(200);
    $("#editPostPopup").fadeIn(200);
}

function cancelEditPost()
{
    $("#modalOverlay").fadeOut(200);
    $("#editPostPopup").fadeOut(200);
}

function revertPost(postId)
{
    var result = confirm("Are you sure you want to revert this post?");

    if (result == true)
    {
        clearInterval(refreshInterval);

        //var sessionId = queryString("id");
        var parameters = '{"sessionId": ' + sessionId + ', "postId": ' + postId + ', "lastPostId": ' + lastPostId + ', "lastUpdateTime": "' + lastUpdateTime + '" }';
        var outerHeight = $("#divPostContainer").outerHeight();
        var scrollTop = $("#divPostContainer").scrollTop();
        var scrollHeight = $("#divPostContainer").prop("scrollHeight");
        var scrollToEnd = (scrollHeight - scrollTop == outerHeight);

        $.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: rootPath + "/RevertPost",
            data: parameters,
            dataType: "json",
            async: true,
            success: function (data)
            {
                var jsonData = eval(data)[0];
                handleNewPosts(jsonData, scrollToEnd);
                refreshInterval = setInterval(pageRefresh, 3000);
            },
            error: function (jqXHR, textStatus, errorThrown)
            {
                alert("Unable to revert post at this time.");
                refreshInterval = setInterval(pageRefresh, 3000);
            }
        });          
    }
}

function btnEditPostSubmit_click(postId)
{
    $("#btnEditPostSubmit").prop("disabled", true);
    var text = $("#txtEditPost").val().trim();
    if (text.length > 0)
    {
        var result = confirm("Are you sure you want to edit this post?");

        if (result)
        {
            $("#txtEditPost").val("");
            editedPostSubmitted(postId, text);
            cancelEditPost();
        }
    }
}

function editedPostSubmitted(postId, text)
{
    clearInterval(refreshInterval);

    var cleanedText = text.replace(/"/g, '&quote;');

    //var sessionId = queryString("id");
    var parameters = '{"sessionId": ' + sessionId + ', "postId": ' + postId + ', "lastPostId": ' + lastPostId + ', "text": "' + cleanedText + '", "lastUpdateTime": "' + lastUpdateTime + '" }';
    var outerHeight = $("#divPostContainer").outerHeight();
    var scrollTop = $("#divPostContainer").scrollTop();
    var scrollHeight = $("#divPostContainer").prop("scrollHeight");
    var scrollToEnd = (scrollHeight - scrollTop == outerHeight);

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: rootPath + "/EditTextPost",
        data: parameters,
        dataType: "json",
        async: true,
        success: function (data)
        {
            var jsonData = eval(data)[0];
            handleNewPosts(jsonData, scrollToEnd);
            refreshInterval = setInterval(pageRefresh, 3000);
        },
        error: function (jqXHR, textStatus, errorThrown)
        {
            alert("Unable to edit post at this time.");
            refreshInterval = setInterval(pageRefresh, 3000);
            $("#btnEditPostSubmit").prop("disabled", false);
        }
    });
}

function showEditRecipientsPopup()
{
    $("#divRecipientsPopupContent").attr("style", "display:none;");
    $("#divRecipientsPopupLoading").attr("style", "display:block;");

    $("#modalOverlay").fadeIn(200);
    $("#editRecipientsPopup").fadeIn(200);

    if (recipients.length == 0)
    {
        $("#chkEveryone").prop("checked", true);
        $("#divRecipientsOverlay").attr("style", "display:block;");
    }
    else
    {
        $("#chkEveryone").prop("checked", false);
        $("#divRecipientsOverlay").attr("style", "display:none;");
    }

    setupRecipients();
}

function cancelEditRecipients()
{
    $("#modalOverlay").fadeOut(200);
    $("#editRecipientsPopup").fadeOut(200);
}

function toggleRecipientsEnabled()
{
    if ($("#chkEveryone").is(":checked"))
    {
        $("#divRecipientsOverlay").stop().fadeIn(400);
    }
    else
    {
        $("#divRecipientsOverlay").stop().fadeOut(400);
    }
}

function setupRecipients()
{
    $("#btnUpdateRecipients").prop("disabled", true);
    var recipientString = recipients.join(",");
    //var sessionId = queryString("id");
    var parameters = '{"sessionId": ' + sessionId + ', "recipientString": "' + recipientString + '" }';

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: rootPath + "/GetRecipientList",
        data: parameters,
        dataType: "json",
        async: false,
        success: function (data)
        {
            var jsonData = eval(data);
            $("#divRecipients").html("");
            if (jsonData.length > 0)
            {
                $.each(jsonData, function (index, recipient)
                {
                    $("#divRecipients").append(recipient.Content);
                });
                $("#btnUpdateRecipients").prop("disabled", false);
                $("#divRecipientsPopupContent").fadeIn(400);
                $("#divRecipientsPopupLoading").attr("style", "display:none");
            }
            else
            {
                $("#editRecipientsPopup").attr("style", "display:none");
                alert("There are no other players in this campaign.");
                $("#modalOverlay").fadeOut(200);
            }
        },
        error: function (jqXHR, textStatus, errorThrown)
        {
            alert("Unable to edit recipients at this time.");
            cancelEditRecipients();
        }
    });
}

function updateRecipients()
{
    $("#modalOverlay").fadeOut(200);
    $("#editRecipientsPopup").fadeOut(200);

    recipients = new Array();
    if ($("#chkEveryone").is(":checked"))
    {
        $("#spanRecipients").html("Everyone");
    }
    else
    {
        var names = new Array();
        $(".RecipientCheckBox").each(function (index)
        {
            if ($(this).is(":checked"))
            {
                recipients.push($(this).attr("player_id"));
                names.push($(this).attr("player_name"));
            }
        });
        if (recipients.length > 0)
        {
            $("#spanRecipients").html(names.join(", "));
        }
        else
        {
            $("#spanRecipients").html("Everyone");
        }
    }
}

function clearRecipients()
{
    $("#spanRecipients").html("Everyone");
    recipients = new Array();
}

function ddlPostAs_Changed()
{
    setupCharacterDetails();
}

function setupCharacterDetails()
{
    var characterId = -1;
    if ($("#ddlPostAs option").size() > 0)
    {
        characterId = $("#ddlPostAs").val();
    }

    if (characterId > 0)
    {
        $("#imgCharacter").attr("src", rootPath + "/image/" + characterId);
       // $("#imgCharacter").css("cursor", "pointer");
        $("#btnEditCharacter").prop("disabled", false);
        $("#btnEditCharacter").attr("onclick", "editCharacterSheet(" + characterId + ");");
        //$("#imgCharacter").attr("onclick", "viewCharacter(" + characterId + ", '" + $("#ddlPostAs option:selected").text() + "');");
    }
    else
    {
        if (characterId == 0)
        {
            $("#imgCharacter").attr("src", "/content/images/roleplayforum/gm.jpg");
        }
        else
        {
            $("#imgCharacter").attr("src", "/content/images/roleplayforum/default_character.jpg");
        }
        //$("#imgCharacter").css("cursor", "default");
        $("#btnEditCharacter").prop("disabled", true);
        $("#btnEditCharacter").removeAttr("onclick");
        //$("#imgCharacter").removeAttr("onclick");
    }
}

//function cancelEditCharacterSheet()
//{
//    $("#modalOverlay").fadeOut(200);
//    $("#editCharacterSheetPopup").fadeOut(200);
//}

//function editCharacterSheet(characterId)
//{
//    $("#btnSubmitCharacterSheet").prop("disabled", false);

//    $("#divEditCharacterSheetPopupContent").attr("style", "display:none;");
//    $("#divEditCharacterSheetPopupLoading").attr("style", "display:block;");

//    $("#btnSubmitCharacterSheet").attr("onclick", "updateCharacterSheet(" + characterId + ");");

//    $("#modalOverlay").fadeIn(200);
//    $("#editCharacterSheetPopup").fadeIn(200);

//    setupEditCharacterSheet(characterId);
//}

//function setupEditCharacterSheet(characterId)
//{
//    var parameters = '{"characterId": ' + characterId + ' }';

//    $.ajax({
//        type: "POST",
//        contentType: "application/json; charset=utf-8",
//        url: rootPath + "/GetEditableCharacterSheet",
//        data: parameters,
//        dataType: "json",
//        async: true,
//        success: function (data)
//        {
//            var jsonData = eval(data);
//            $("#divCharacterSheetContainer").html(jsonData);
//            $("#divEditCharacterSheetPopupContent").fadeIn(400);
//            $("#divEditCharacterSheetPopupLoading").attr("style", "display:none");
//            $("#btnSubmitCharacterSheet").prop("disabled", false);

//        },
//        error: function (jqXHR, textStatus, errorThrown)
//        {
//            alert("Unable to edit character sheet at this time.");
//            cancelEditCharacterSheet();
//        }
//    });
//}

//function updateCharacterSheet(characterId)
//{
//    $("#btnSubmitCharacterSheet").prop("disabled", true);

//    var sheet = new Object();
//    sheet.CharacterSheetStyle = parseInt($("#divEditableCharacterSheet").attr("sheetType"));
//    sheet.IsPrivate = $("#chkPrivateSheet").is(":checked");
//    sheet.XP = $("#txtXp").val().trim();
//    sheet.Chips = $("#txtChips").val().trim();
//    sheet.CurrentDamage = $("#txtCurrentDamage").val().trim();
//    sheet.Insanity = $("#txtInsanity").val().trim();
    

//    if (isNaN($("#txtBaseHitCapacity").val().trim()) || $("#txtBaseHitCapacity").val().trim() == "")
//    {
//        sheet.BaseHitCapacity = 6;
//    }
//    else
//    {
//        sheet.BaseHitCapacity = parseInt($("#txtBaseHitCapacity").val());
//    }

//    sheet.Skills = new Object();
//    $(".SkillRow").each(function (index)
//    {
//        var skillName = $(this).find(".SkillRowTitle").html().trim();
//        var skillValue = $(this).find(".SkillTextBox").val().trim();

//        sheet.Skills[skillName] = skillValue;
//    });

//    sheet.Traits = new Object();
//    $(".TraitRow").each(function (index)
//    {
//        var traitName = $(this).find(".TraitRowTitle").html().trim();
//        var selectedValue = "";
//        for (var i = 1; i <= 3; i++)
//        {
//            if ($("[trait_name='" + traitName + "'][trait_value='" + i + "']").attr("dot_selected") == "true")
//            {
//                selectedValue = i.toString();
//            }
//        }

//        sheet.Traits[traitName] = selectedValue;
//    });

//    sheet.Specialities = new Object();
//    $(".SpecialityRow").each(function (index)
//    {
//        var specName = $(this).find(".SpecialityNameTextBox").val().trim();
//        var specValue = $(this).find(".SpecialityValueTextBox").val().trim();

//        if (specName != "")
//        {
//            sheet.Specialities[specName] = specValue;
//        }
//    });

//    sheet.Other = new Object();
//    $(".OtherRow").each(function (index)
//    {
//        var otherName = $(this).find(".OtherNameTextBox").val().trim();
//        var otherValue = $(this).find(".OtherValueTextBox").val().trim();

//        if (otherName != "")
//        {
//            sheet.Other[otherName] = otherValue;
//        }
//    });

//    sheet.MinorWounds = new Object();
//    sheet.SeriousWounds = new Object();
//    $("[wound_row]").each(function (index)
//    {
//        var locationName = $(this).attr("wound_row").trim();
//        var minorValue = $(this).find(".MinorWoundTextBox").val().trim();
//        var seriousValue = $(this).find(".SeriousWoundTextBox").val().trim();

//        sheet.MinorWounds[locationName] = minorValue;
//        sheet.SeriousWounds[locationName] = seriousValue;
//    });

//    var jsonSheet = JSON.stringify(sheet);
//    submitCharacterSheet(characterId, jsonSheet);
//}

//function submitCharacterSheet(characterId, jsonSheet)
//{
//    var parameters = "{'characterId': " + characterId + ", 'jsonSheet': '" + jsonSheet + "' }";

//    $.ajax({
//        type: "POST",
//        contentType: "application/json; charset=utf-8",
//        url: rootPath + "/SubmitCharacterSheet",
//        data: parameters,
//        dataType: "json",
//        async: true,
//        success: function (data)
//        {
//            $("#modalOverlay").fadeOut(200);
//            $("#editCharacterSheetPopup").fadeOut(200);

//            if ($("#CharacterPopOver").attr("shown") == "true")
//            {
//                viewCharacter(characterId, $("#CharacterPopOverName").html());
//            }
//        },
//        error: function (jqXHR, textStatus, errorThrown)
//        {
//            alert("Unable to edit character sheet at this time.");
//            $("#btnSubmitCharacterSheet").prop("disabled", false);
//        }
//    });
//}

function activateCharacterTab(tab)
{
    if (tab == 1)
    {
        $("#tabCharacterProfile").attr("class","CharacterTabEnabled");
        $("#tabCharacterSheet").attr("class","CharacterTabDisabled");
        $("#CharacterPopOverSheet").attr("style", "display:none;");
        $("#CharacterPopOverDescription").fadeIn(400);
    }
    else
    {
        $("#tabCharacterProfile").attr("class","CharacterTabDisabled");
        $("#tabCharacterSheet").attr("class","CharacterTabEnabled");
        $("#CharacterPopOverSheet").fadeIn(400);
        $("#CharacterPopOverDescription").attr("style", "display:none;");
    }
}