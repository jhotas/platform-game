using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct3D9;
using System;

namespace platformGame
{
    public class Enemy
    {
        private GraphicsDevice _graphicsDevice;
        private Texture2D pixelTexture;
        public Texture2D _sprite;
        public Vector2 _position;
        private float _speed;
        public bool _isAlive = true;
        private int _frameWidth;
        private int _frameHeight;
        private int _currentFrame;
        private TimeSpan _timer;
        private int _animationSpeed = 50;
        private int _totalFrames;
        private bool _movingRight = true;
        private int _movementDistance = 200; // Distância que o inimigo irá percorrer antes de mudar de direção
        private int _movedDistance = 0; // Distância percorrida desde a última mudança de direção

        public Rectangle _hitbox;
        public Rectangle Hitbox => _hitbox;

        public Enemy(Texture2D sprite, Vector2 position, float speed, int totalFrames, GraphicsDevice graphicsDevice)
        {
            _sprite = sprite;
            _position = position;
            _speed = speed;
            _totalFrames = totalFrames;
            _frameWidth = sprite.Width / totalFrames;
            _frameHeight = sprite.Height;
            _hitbox = new Rectangle((int)position.X, (int)position.Y, _frameWidth, _frameHeight);
            _graphicsDevice = graphicsDevice;
            pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
        }

        public void Update(GameTime gameTime)
        {
            _timer += gameTime.ElapsedGameTime;
            if (_timer.TotalMilliseconds > _animationSpeed)
            {
                _currentFrame++;
                if (_currentFrame >= _totalFrames)
                {
                    _currentFrame = 0;
                }
                _timer = TimeSpan.Zero;
            }

            if (_movingRight)
            {
                _position.X += _speed;
                _movedDistance += (int)_speed;
                if (_movedDistance >= _movementDistance)
                {
                    _movingRight = false;
                    _movedDistance = 0;
                }
            }
            else
            {
                _position.X -= _speed;
                _movedDistance += (int)_speed;
                if (_movedDistance >= _movementDistance)
                {
                    _movingRight = true;
                    _movedDistance = 0;
                }
            }

            // Atualiza a posição da hitbox para seguir o inimigo
            _hitbox.X = (int)_position.X;
            _hitbox.Y = (int)_position.Y;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Determina o efeito de espelhamento horizontal com base na direção do movimento
            SpriteEffects flipEffect = _movingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // spriteBatch.Draw(pixelTexture, _hitbox, Color.Red);

            Rectangle sourceRectangle = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);
            spriteBatch.Draw(_sprite, _position, sourceRectangle, Color.White, 0, Vector2.Zero, 1.0f, flipEffect, 0);
        }
    }
}
