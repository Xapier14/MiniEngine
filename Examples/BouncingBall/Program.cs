using MiniEngine;
using MiniEngine.Components;
using MiniEngine.Systems;
using MiniEngine.Graphics;
using MiniEngine.Resources;

namespace BouncingBall;

class Program
{
    static void Main(string[] args)
    {
        // Get the game engine instance
        var gameEngine = GameContext.GetGameEngine();
        
        // Add initializer to set up the game
        gameEngine.AddInitializer(() =>
        {
            try
            {
                // Load the asset pack
                Resources.LoadAssetPack("Assets.mea");
                
                // Create and set up the game scene
                var gameScene = new GameScene();
                SceneManager.SetActiveScene(gameScene);
                
                Console.WriteLine("Bouncing Ball example initialized successfully!");
                return false; // No error
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during initialization: {ex.Message}");
                return true; // Error occurred
            }
        });
        
        // Initialize and run the engine
        gameEngine.Initialize();
        gameEngine.Run();
    }
}

/// <summary>
/// Main game scene that contains our bouncing ball
/// </summary>
public class GameScene : Scene
{
    public GameScene() : base("BouncingBall")
    {
        SetupBall();
        SetupSystems();
    }
    
    private void SetupBall()
    {
        // Create the ball entity
        var ball = CreateEntity("Ball");
        
        // Add Transform component for position and size
        var transform = ball.AddComponent<Transform>();
        transform.Position = new Vector2(400, 300); // Center of typical screen
        transform.Scale = new Vector2(1.0f, 1.0f);
        
        // Add Motion component for movement
        var motion = ball.AddComponent<Motion>();
        motion.Velocity = new Vector2(200, 150); // Pixels per second
        motion.MaxSpeed = 400f;
        
        // Add Sprite component for rendering
        var sprite = ball.AddComponent<Sprite>();
        sprite.TexturePath = "Sprites/ball.png";
        sprite.Size = new Size(32, 32);
        
        // Add our custom bouncing behavior
        ball.AddComponent<BouncingBehavior>();
        
        Console.WriteLine("Ball entity created with Transform, Motion, Sprite, and BouncingBehavior components");
    }
    
    private void SetupSystems()
    {
        // Register our custom bouncing system
        SystemService.RegisterSystem<BouncingSystem>();
        
        Console.WriteLine("BouncingSystem registered");
    }
}

/// <summary>
/// Component that holds bouncing-specific data
/// </summary>
public class BouncingBehavior : Component
{
    public float BounceDamping { get; set; } = 0.95f; // Slight energy loss on bounce
    public bool BounceOnEdges { get; set; } = true;
    public Vector2 ScreenBounds { get; set; } = new Vector2(800, 600); // Default screen size
}

/// <summary>
/// System that handles bouncing physics
/// </summary>
public class BouncingSystem : ComponentSystem<BouncingBehavior>
{
    protected override void ProcessComponent(Entity entity, BouncingBehavior bouncing)
    {
        if (!bouncing.BounceOnEdges) return;
        
        var transform = entity.GetComponent<Transform>();
        var motion = entity.GetComponent<Motion>();
        
        if (transform == null || motion == null) return;
        
        var sprite = entity.GetComponent<Sprite>();
        var ballSize = sprite?.Size ?? new Size(32, 32);
        
        bool bounced = false;
        
        // Check horizontal bounds
        if (transform.Position.X <= ballSize.Width / 2 || 
            transform.Position.X >= bouncing.ScreenBounds.X - ballSize.Width / 2)
        {
            motion.Velocity = new Vector2(-motion.Velocity.X * bouncing.BounceDamping, motion.Velocity.Y);
            
            // Keep ball within bounds
            transform.Position = new Vector2(
                Math.Clamp(transform.Position.X, ballSize.Width / 2, bouncing.ScreenBounds.X - ballSize.Width / 2),
                transform.Position.Y
            );
            
            bounced = true;
        }
        
        // Check vertical bounds
        if (transform.Position.Y <= ballSize.Height / 2 || 
            transform.Position.Y >= bouncing.ScreenBounds.Y - ballSize.Height / 2)
        {
            motion.Velocity = new Vector2(motion.Velocity.X, -motion.Velocity.Y * bouncing.BounceDamping);
            
            // Keep ball within bounds
            transform.Position = new Vector2(
                transform.Position.X,
                Math.Clamp(transform.Position.Y, ballSize.Height / 2, bouncing.ScreenBounds.Y - ballSize.Height / 2)
            );
            
            bounced = true;
        }
        
        if (bounced)
        {
            Console.WriteLine($"Ball bounced at position ({transform.Position.X:F1}, {transform.Position.Y:F1})");
        }
    }
}