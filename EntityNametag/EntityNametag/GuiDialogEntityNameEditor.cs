using System;
using Vintagestory.API.Client;

namespace EntityNametag;

public class GuiDialogEntityNameEditor : GuiDialog
{
    public GuiDialogEntityNameEditor(ICoreClientAPI capi, string initialText, Action<string> onNameEdited) : base(capi)
    {
        Compose(initialText, onNameEdited);
    }

    public override string ToggleKeyCombinationCode { get; } = "entitynameeditorkeycombo";

    private void Compose(string initialText, Action<string> onNameEdited)
    {
        var textBounds = ElementStdBounds.Rowed(0.4f, 0, EnumDialogArea.LeftFixed).WithFixedWidth(500);
        var bgBounds = ElementStdBounds.DialogBackground()
            .WithFixedPadding(GuiStyle.ElementToDialogPadding, GuiStyle.ElementToDialogPadding);
        var util = new TextDrawUtil();
        var font = CairoFont.WhiteSmallText();

        var y = (float)util.GetMultilineTextHeight(font, initialText, textBounds.fixedWidth);

        SingleComposer =
            capi.Gui
                .CreateCompo("entitynameeditor", ElementStdBounds.AutosizedMainDialog)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(LangString("editortitle"), () => TryClose())
                .BeginChildElements(bgBounds)
                .AddTextInput(ElementBounds.Fixed(0, 50, 400, 30),
                    newText =>
                    {
                        SingleComposer.GetButton("confirmbutton").Enabled = !string.IsNullOrWhiteSpace(newText);
                    }, null, "newentityname")
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
                        onNameEdited(SingleComposer.GetTextInput("newentityname").GetText());
                        TryClose();
                        return true;
                    },
                    ElementStdBounds.MenuButton((y + 80) / 80f).WithAlignment(EnumDialogArea.RightFixed)
                        .WithFixedPadding(6), EnumButtonStyle.Normal, "confirmbutton")
                .EndChildElements()
                .Compose()
            ;

        SingleComposer.GetTextInput("newentityname").SetValue(initialText);
    }
}