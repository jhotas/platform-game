using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;


namespace platformGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Variáveis para a spritesheet
        private Texture2D _playerSpritesheet;
        private Texture2D _playerRunSpritesheet;
        private int _frameWidth;
        private int _frameHeight;
        private int _totalFrames;
        private int _currentFrame;
        private TimeSpan _timer;
        private TimeSpan _runTimer;
        private int _animationSpeed = 100;
        private int _runAnimationSpeed = 100;

        // Variáveis para a física
        private float _gravity = 0.5f;
        private float _verticalSpeed = 0f;
        private float _groundLevel = 300f; // Ajuste conforme necessário para o seu jogo

        // Variáveis para o jogador
        private Vector2 _playerPosition;
        private float _playerSpeed = 2f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Inicializa a posição do jogador
            _playerPosition = new Vector2(100, 100);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Carrega a spritesheet do jogador
            _playerSpritesheet = Content.Load<Texture2D>("player_idle");
            _playerRunSpritesheet = Content.Load<Texture2D>("player_run");

            // Define as dimensões do quadro de animação
            _frameWidth = 32;
            _frameHeight = 32;

            // Define o número total de frames na spritesheet
            _totalFrames = 11; // Defina o número correto de frames na sua spritesheet
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Lógica de movimentação do jogador
            var keyboardState = Keyboard.GetState();
            bool isMoving = false;

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                _playerPosition.X -= _playerSpeed;
                isMoving = true;
            }
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                _playerPosition.X += _playerSpeed;
                isMoving = true;
            }
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                _playerPosition.Y -= _playerSpeed;
                isMoving = true;
            }

            // Aplicar gravidade
            _verticalSpeed += _gravity;
            _playerPosition.Y += _verticalSpeed;

            // Checar colisão com o chão
            if (_playerPosition.Y >= _groundLevel)
            {
                _playerPosition.Y = _groundLevel;
                _verticalSpeed = 0; // Reseta a velocidade vertical quando atinge o chão
            }

            // Atualiza a animação
            if (isMoving)
            {
                UpdateAnimation(gameTime, _playerRunSpritesheet, ref _runTimer, _runAnimationSpeed, 12); // 12 frames para a animação de corrida
            }
            else
            {
                UpdateAnimation(gameTime, _playerSpritesheet, ref _timer, _animationSpeed, 11); // 11 frames para a animação de idle
            }

            base.Update(gameTime);
        }

        private void UpdateAnimation(GameTime gameTime, Texture2D spritesheet, ref TimeSpan timer, int animationSpeed, int totalFrames)
        {
            // Atualiza o timer da animação
            timer += gameTime.ElapsedGameTime;

            if (timer.TotalMilliseconds > animationSpeed)
            {
                // Avança para o próximo frame
                _currentFrame++;
                if (_currentFrame >= totalFrames)
                {
                    _currentFrame = 0; // Volta ao primeiro frame quando chegar ao último
                }

                timer = TimeSpan.Zero; // Reseta o timer
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.Right) ||
                Keyboard.GetState().IsKeyDown(Keys.Up) || Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                DrawFrame(_playerRunSpritesheet, 32, 32, 12); // 32x32 e 12 frames para a animação de corrida
            }
            else
            {
                DrawFrame(_playerSpritesheet, 32, 32, 11); // 32x32 e 11 frames para a animação de idle
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawFrame(Texture2D spritesheet, int frameWidth, int frameHeight, int totalFrames)
        {
            // Calcula o retângulo correspondente ao frame atual
            Rectangle sourceRectangle = new Rectangle(_currentFrame * frameWidth, 0, frameWidth, frameHeight);

            // Desenha o frame atual
            _spriteBatch.Draw(spritesheet, _playerPosition, sourceRectangle, Color.White);
        }
    }
}