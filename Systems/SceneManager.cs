using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EscapeSpaceStation.Systems
{
    /// <summary>Base class every game scene (menu, gameplay, victory, defeat...) inherits from.</summary>
    public abstract class Scene
    {
        protected readonly GameServices Services;
        protected readonly SceneManager Manager;

        protected Scene(GameServices services, SceneManager manager)
        {
            Services = services;
            Manager = manager;
        }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }

        public abstract void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse, KeyboardState keys, KeyboardState prevKeys);
        public abstract void Draw(SpriteBatch spriteBatch, Rectangle screenBounds);
    }

    /// <summary>Simple stack-free scene switcher: exactly one active scene at a time.</summary>
    public class SceneManager
    {
        public Scene CurrentScene { get; private set; }

        public void ChangeScene(Scene next)
        {
            CurrentScene?.OnExit();
            CurrentScene = next;
            CurrentScene?.OnEnter();
        }
    }
}
