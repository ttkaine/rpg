
function addSpeciality()
{
    if ($(".SpecialityRow").length < 25)
    {
        var newItem = $('<div class="SpecialityRow" style="display:none;"></div>').append('<span class="SpecialityRowTitle"><input type="text" class="SpecialityNameTextBox" value="" /></span><span class="SpecialityRowContent"><input type="text" class="SpecialityValueTextBox" value="" maxlength="3" /></span><span class="RemoveSpecialityRow" onclick="removeSpecialityRow(this);"></span><div class="Clear"></div>');
        $("#divSpecialityRowContainer").append(newItem);
        $(newItem).fadeIn(400);
    }
    else
    {
        alert("Maximum 25 rows.");
    }
}

function addOther()
{
    if ($(".OtherRow").length < 25)
    {
        var newItem = $('<div class="OtherRow" style="display:none;"></div>').append('<span class="OtherRowTitle"><input type="text" class="OtherNameTextBox" value="" /></span><span class="OtherRowContent"><input type="text" class="OtherValueTextBox" value="" maxlength="3" /></span><span class="RemoveOtherRow" onclick="removeOtherRow(this);"></span><div class="Clear"></div>');
        $("#divOtherRowContainer").append(newItem);
        $(newItem).fadeIn(400);
    }
    else
    {
        alert("Maximum 25 rows.");
    }
}

function updateDotSelector(traitValue, traitName)
{
    if ($("[trait_name='" + traitName + "'][trait_value='" + traitValue + "']").attr("dot_selected") == "true")
    {
        clearDotSelectorRow(traitName);
    }
    else
    {
        $("[trait_name='" + traitName + "']").each(function (index)
        {
            $(this).attr("dot_selected", "false");
            var itemValue = parseInt($(this).attr("trait_value"));
            if (itemValue <= parseInt(traitValue))
            {
                $(this).attr("class", "DotSelectorChecked");
            }
            else
            {
                $(this).attr("class", "DotSelectorUnchecked");
            }
        });
        $("[trait_name='" + traitName + "'][trait_value='" + traitValue + "']").attr("dot_selected", "true");
    }
    updateHitCapacity();
}

function clearDotSelectorRow(traitName)
{
    $("[trait_name='" + traitName + "']").each(function (index)
    {
        $(this).attr("class", "DotSelectorUnchecked");
        $(this).attr("dot_selected", "false");
    });
}

function updateHitCapacity()
{
    var baseCapacity = 6;
    var enteredValue = $("#txtBaseHitCapacity").val();
    if (isNaN(enteredValue))
    {
        $("#txtBaseHitCapacity").val("6");
    }
    else
    {
        if (enteredValue != '')
        {
            baseCapacity = parseInt($("#txtBaseHitCapacity").val());
        }
        else
        {
            baseCapacity = 6;
        }
    }
    var increments = 0;

    $("[trait_value]").each(function (index)
    {
        if ($(this).attr("isHitCapacityMultiplier") == "true" && $(this).attr("dot_selected") == "true")
        {
            increments += parseInt($(this).attr("trait_value"));
        }
    });

    $("#spanMinorHitCapacity").html(baseCapacity + increments);
    $("#spanSeriousHitCapacity").html((baseCapacity + increments) * 2);
    $("#spanCriticalHitCapacity").html((baseCapacity + increments) * 3);
}

function removeSpecialityRow(sender)
{
    $(sender).parent().fadeOut(400, function ()
    {
        $(this).remove();
    });
}

function removeOtherRow(sender)
{
    $(sender).parent().fadeOut(400, function ()
    {
        $(this).remove();
    });
}

function baseHitCapacityChanged()
{
    updateHitCapacity();
}