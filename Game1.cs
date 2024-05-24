using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing.Text;
using System.IO;


namespace platformGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private Dictionary<Vector2, int> chao;
        private Dictionary<Vector2, int> collisions;
        private Texture2D textureAtlas;
        private Texture2D pixelTexture;

        // Variáveis para a spritesheet
        private Texture2D _playerSpritesheet;
        private Texture2D _playerRunSpritesheet;
        private Texture2D _playerJumpSpritesheet;
        private Texture2D _playerDoubleJumpSpritesheet;
        private Texture2D _playerFallSpritesheet;

        private int _frameWidth;
        private int _frameHeight;
        private int _currentFrame;
        private TimeSpan _timer;
        private TimeSpan _runTimer;
        private int _animationSpeed = 100;

        // Variáveis para a física
        private float _gravity = 0.15f;
        private float _jumpSpeed = 5f;
        private float _verticalSpeed = 0f;
        private float _groundLevel = 350f; // Ajuste conforme necessário para o seu jogo

        // Variáveis para o jogador
        private Rectangle _playerHitbox;
        private List<Rectangle> intersections;
        private Vector2 _playerPosition;
        private float _playerSpeed = 2f;
        private bool _jumpInitiated;
        private bool _doubleJumpAvailable;
        private bool isDoubleJumping;
        private bool isJumping = false;
        private bool isFalling = false;

        private int _score = 0;

        // Variáveis para o inimigo
        private Enemy _enemy;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            chao = LoadMap("../../../Data/tilesetmap_tile.csv");
            collisions = LoadMap("../../../Data/tilesetmap_tile_collisions.csv");
            intersections = new();
        }

        private Dictionary<Vector2, int> LoadMap(string filepath)
        {
            Dictionary<Vector2, int> result = new();

            StreamReader reader = new(filepath);

            int y = 0;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] items = line.Split(',');

                for (int x = 0; x < items.Length; x++)
                {
                    if (int.TryParse(items[x], out int value))
                    {
                        if (value > -1)
                        {
                            result[new Vector2(x, y)] = value;
                        }
                    }
                }

                y++;
            }

            return result;
        }

        protected override void Initialize()
        {
            // Inicializa a posição do jogador
            _playerPosition = new Vector2(120, 340);
            _playerHitbox = new Rectangle(100, 100, 32, 32);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _font = Content.Load<SpriteFont>("pixelFont");

            // Carregar a textura branca de 1x1 pixel
            pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });

            // Carrega a spritesheet do jogador
            _playerSpritesheet = Content.Load<Texture2D>("player_idle");
            _playerRunSpritesheet = Content.Load<Texture2D>("player_run");
            _playerJumpSpritesheet = Content.Load<Texture2D>("player_jump");
            _playerFallSpritesheet = Content.Load<Texture2D>("player_fall");
            _playerDoubleJumpSpritesheet = Content.Load<Texture2D>("player_double_jump");

            _enemy = new Enemy(Content.Load<Texture2D>("enemy_sprite"), new Vector2(200, 335), 1f, totalFrames: 16, GraphicsDevice);

            // Carrega a spritesheet do cenário
            textureAtlas = Content.Load<Texture2D>("tile_map");

            // Define as dimensões do quadro de animação
            _frameWidth = 32;
            _frameHeight = 32;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            intersections = getIntersectingTilesHorizontal(_playerHitbox);
            intersections = getIntersectingTilesVertical(_playerHitbox);

            // Lógica de movimentação do jogador
            var keyboardState = Keyboard.GetState();
            bool isMoving = false;

            // Atualização da caixa de colisão do jogador
            _playerHitbox.X = (int)_playerPosition.X;
            _playerHitbox.Y = (int)_playerPosition.Y;

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

            // Lógica de pulo e queda
            if (keyboardState.IsKeyDown(Keys.Up) && !isJumping && !isFalling && !_jumpInitiated)
            {
                isJumping = true;
                _jumpInitiated = true;
                _verticalSpeed = -_jumpSpeed;
                _doubleJumpAvailable = true; // Permite pulo duplo
            }
            else if ((keyboardState.IsKeyDown(Keys.Up) && isFalling && _doubleJumpAvailable))
            {
                isDoubleJumping = true;
                _currentFrame = 0;
                _verticalSpeed = -_jumpSpeed;
                _doubleJumpAvailable = false;
            }

            if (!keyboardState.IsKeyDown(Keys.Up))
            {
                _jumpInitiated = false;
            }

            if (isJumping || isFalling || isDoubleJumping)
            {
                _playerPosition.Y += _verticalSpeed;
                _verticalSpeed += _gravity;

                if (_verticalSpeed > 0)
                {
                    isJumping = false;
                    if (isDoubleJumping)
                    {
                        isDoubleJumping = false;
                        isFalling = true;
                    }
                    else
                    {
                        isFalling = true;
                    }
                }

                // Verifica se o jogador atingiu o chão
                if (_playerPosition.Y >= 350)
                {
                    _playerPosition.Y = 350;
                    isFalling = false;
                    _verticalSpeed = 0;
                    _doubleJumpAvailable = false;
                }
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

            if (_playerHitbox.Intersects(_enemy.Hitbox))
            {
                _enemy._isAlive = false;
                _score += 100;
            }

            // Atualiza a animação
            if (isJumping)
            {
                _currentFrame = 0;
            }
            else if (isDoubleJumping)
            {
                UpdateAnimation(gameTime, _playerDoubleJumpSpritesheet, ref _timer, _animationSpeed, 6);
            }
            else if (isFalling)
            {
                _currentFrame = 0;
            }
            else if (isMoving)
            {
                UpdateAnimation(gameTime, _playerRunSpritesheet, ref _timer, _animationSpeed, 12);
            }
            else
            {
                UpdateAnimation(gameTime, _playerSpritesheet, ref _timer, _animationSpeed, 11); // 11 frames para a animação de idle
            }

            // Atualiza o inimigo
            _enemy.Update(gameTime);

            base.Update(gameTime);
        }

        public List<Rectangle> getIntersectingTilesHorizontal(Rectangle target)
        {
            List<Rectangle> intersections = new();

            int widthInTiles = (target.Width - (target.Width % 64)) / 64;
            int heightInTiles = (target.Height - (target.Height % 64)) / 64;

            for (int x = 0; x <= widthInTiles; x++)
            {
                for (int y = 0; y <= heightInTiles; y++)
                {
                    intersections.Add(new Rectangle(
                        (target.X + x * 64) / 64,
                        (target.Y + y * (64 - 1)) / 64,
                        64,
                        64
                    ));
                }
            }
            return intersections;
        }

        public List<Rectangle> getIntersectingTilesVertical(Rectangle target)
        {
            List<Rectangle> intersections = new();

            int widthInTiles = (target.Width - (target.Width % 64)) / 64;
            int heightInTiles = (target.Height - (target.Height % 64)) / 64;

            for (int x = 0; x <= widthInTiles; x++)
            {
                for (int y = 0; y <= heightInTiles; y++)
                {
                    intersections.Add(new Rectangle(
                        (target.X + x * (64 - 1)) / 64,
                        (target.Y + y * 64) / 64,
                        64,
                        64
                    ));
                }
            }
            return intersections;
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
                    _currentFrame = 0;
                }

                timer = TimeSpan.Zero; // Reseta o timer
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            int displayTilesize = 26;
            int numTilesPerRow = 10;
            int pixelTilesize = 16;

            foreach (var item in chao)
            {
                Rectangle drect = new(
                    (int)item.Key.X * displayTilesize,
                    (int)item.Key.Y * displayTilesize,
                    displayTilesize,
                    displayTilesize
                );

                int x = item.Value % numTilesPerRow;
                int y = item.Value / numTilesPerRow;

                Rectangle src = new(
                    x * pixelTilesize,
                    y * pixelTilesize,
                    pixelTilesize,
                    pixelTilesize
                );

                _spriteBatch.Draw(textureAtlas, drect, src, Color.White);
            }

            // Calculate the flip effect
            SpriteEffects flipEffect = SpriteEffects.None;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                flipEffect = SpriteEffects.FlipHorizontally;
            }

            if (isJumping)
            {
                _spriteBatch.Draw(_playerJumpSpritesheet, _playerPosition, new Rectangle(0, 0, _frameWidth, _frameHeight), Color.White, 0, new Vector2(_frameWidth / 2, _frameHeight / 2), 1.0f, flipEffect, 0);
            }
            else if (isDoubleJumping)
            {
                DrawFrame(_playerDoubleJumpSpritesheet, 32, 32, 10, 0);
            }
            else if (isFalling)
            {
                _spriteBatch.Draw(_playerFallSpritesheet, _playerPosition, new Rectangle(0, 0, _frameWidth, _frameHeight), Color.White, 0, new Vector2(_frameWidth / 2, _frameHeight / 2), 1.0f, flipEffect, 0);
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                DrawFrame(_playerRunSpritesheet, 32, 32, 12, flipEffect);
            }
            else
            {
                DrawFrame(_playerSpritesheet, 32, 32, 11, 0);
            }

            // Calcular a posição correta da hitbox
            int hitboxX = (int)(_playerPosition.X - _playerHitbox.Width / 2);
            int hitboxY = (int)(_playerPosition.Y - _playerHitbox.Height / 2);

            // Desenhar a caixa de colisão do jogador com a posição corrigida
            //_spriteBatch.Draw(pixelTexture, new Rectangle(hitboxX, hitboxY, _playerHitbox.Width, _playerHitbox.Height), Color.Red);

            if (_enemy._isAlive)
            {
                _enemy.Draw(_spriteBatch);
            }

            string scoreText = "Pontos: " + _score;
            Vector2 scorePosition = new Vector2(GraphicsDevice.Viewport.Width - 10 - _font.MeasureString(scoreText).X, 10);
            _spriteBatch.DrawString(_font, scoreText, scorePosition, Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawFrame(Texture2D spritesheet, int frameWidth, int frameHeight, int totalFrames, SpriteEffects flipEffect)
        {
            // Calcula o retângulo correspondente ao frame atual
            Rectangle sourceRectangle = new Rectangle(_currentFrame * frameWidth, 0, frameWidth, frameHeight);

            // Desenha o frame atual
            _spriteBatch.Draw(spritesheet, _playerPosition, sourceRectangle, Color.White, 0, new Vector2(_frameWidth / 2, _frameHeight / 2), 1.0f, flipEffect, 0);
        }
    }
}