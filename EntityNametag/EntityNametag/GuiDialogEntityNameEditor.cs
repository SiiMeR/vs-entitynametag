using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace EntityNametag;

public class GuiDialogEntityNameEditor : GuiDialog
{
    public GuiDialogEntityNameEditor(ICoreClientAPI capi, Entity entity,
        Action<string, bool> onEditFinished)
        : base(capi)
    {
        Compose(entity, onEditFinished);
    }

    public override string ToggleKeyCombinationCode { get; } = "entitynameeditorkeycombo";

    private void Compose(Entity entity, Action<string, bool> onEditFinished)
    {
        var textBounds = ElementStdBounds.Rowed(0.4f, 0, EnumDialogArea.LeftFixed).WithFixedWidth(500);
        var bgBounds = ElementStdBounds.DialogBackground()
            .WithFixedPadding(GuiStyle.ElementToDialogPadding, GuiStyle.ElementToDialogPadding);
        var util = new TextDrawUtil();
        var font = CairoFont.WhiteSmallText();

        var y = (float)util.GetMultilineTextHeight(font, entity.GetName(), textBounds.fixedWidth);

        SingleComposer =
            capi.Gui
                .CreateCompo("entitynameeditor", ElementStdBounds.AutosizedMainDialog)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(LangString("editortitle"), () => TryClose())
                .BeginChildElements(bgBounds)
                .AddTextInput(ElementBounds.Fixed(0, 20, 400, 30),
                    newText =>
                    {
                        SingleComposer.GetButton("confirmbutton").Enabled = !string.IsNullOrWhiteSpace(newText);
                    }, null, "newentityname")
                .AddIf(entity is EntityBoat)
                .AddToggleButton("X", font, null, ElementBounds.Fixed(0, 60, 25, 30),
                    "playerowned")
                .AddStaticText(LangString("playerowned"), font, ElementBounds.Fixed(35, 66, 200, 30))
                .EndIf()
                .AddSmallButton(LangString("editorcancel"), () =>
                    {
                        TryClose();
                        return true;
                    },
                    ElementStdBounds.MenuButton((y + 80) / 80f).WithAlignment(EnumDialogArea.LeftFixed)
                        .WithFixedPadding(6))
                .AddSmallButton(LangString("editorclear"), () =>
                    {
                        SingleComposer.GetTextInput("newentityname").SetValue(string.Empty);
                        return true;
                    },
                    ElementStdBounds.MenuButton((y + 80) / 80f).WithAlignment(EnumDialogArea.CenterTop)
                        .WithFixedPadding(6))
                .AddSmallButton(LangString("editorconfirm"), () =>
                    {
                        onEditFinished(SingleComposer
                            .GetTextInput("newentityname")
                            .GetText(), SingleComposer.GetToggleButton("playerowned")?.On ?? false);
                        TryClose();
                        return true;
                    },
                    ElementStdBounds.MenuButton((y + 80) / 80f).WithAlignment(EnumDialogArea.RightFixed)
                        .WithFixedPadding(6), EnumButtonStyle.Normal, "confirmbutton")
                .EndChildElements()
                .Compose()
            ;

        SingleComposer.GetTextInput("newentityname").SetValue(entity.GetName());

        if (entity is EntityBoat)
        {
            var isAlreadyOwned = entity.WatchedAttributes.GetTreeAttribute("ownedby") != null;
            SingleComposer.GetToggleButton("playerowned")?.SetValue(isAlreadyOwned);
        }
    }
}