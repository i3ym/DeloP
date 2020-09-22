using System;
using osu.Framework.Graphics.UserInterface;

namespace DeloP
{
    public class NumberOnlyTextBox : BasicTextBox
    {
        public event Action OnTextChanged = delegate { };

        public NumberOnlyTextBox()
        {
            BackgroundUnfocused = BackgroundFocused = BackgroundCommit = Colors.ToolSelection;
        }

        protected override bool CanAddCharacter(char character) => char.IsNumber(character);
        protected override void OnTextCommitted(bool textChanged)
        {
            base.OnTextCommitted(textChanged);
            OnTextChanged();
        }
    }
}