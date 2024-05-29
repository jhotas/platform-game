using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace platformGame
{
    public class Trap
    {
        private Texture2D _trapOffSprite;
        private Texture2D _trapOnSprite;
        private Texture2D _currentSprite;
        private bool _isActive;

        private TimeSpan _activationDelay; // Tempo de espera para ativar
        private TimeSpan _deactivationDelay; // Tempo de espera para desativar
        private TimeSpan _elapsedTime;

        public Trap(Texture2D trapOffSprite, Texture2D trapOnSprite, TimeSpan activationDelay, TimeSpan deactivationDelay)
        {
            _trapOffSprite = trapOffSprite;
            _trapOnSprite = trapOnSprite;
            _currentSprite = _trapOffSprite; // Começa desligada
            _isActive = false;

            _activationDelay = activationDelay;
            _deactivationDelay = deactivationDelay;
            _elapsedTime = TimeSpan.Zero;
        }

        public void Update(GameTime gameTime)
        {
            _elapsedTime += gameTime.ElapsedGameTime;

            if (_isActive)
            {
                if (_elapsedTime >= _deactivationDelay)
                {
                    _elapsedTime = TimeSpan.Zero;
                    _isActive = false;
                    _currentSprite = _trapOffSprite;
                }
            }
            else
            {
                if (_elapsedTime >= _activationDelay)
                {
                    _elapsedTime = TimeSpan.Zero;
                    _isActive = true;
                    _currentSprite = _trapOnSprite;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            if (spriteBatch != null && _trapOnSprite != null && _trapOffSprite != null)
            {
                // Desenhar a sprite apropriada com base no estado da armadilha
                Texture2D currentSprite = _isActive ? _trapOnSprite : _trapOffSprite;
                spriteBatch.Draw(currentSprite, position, Color.White);
            }
        }
    }
}
