using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Asteroids.Game_Types
{
    abstract class gameType
    {
        //Simple checking condition to see if the game has been initialized
        private bool initialized = false;
        public bool HasInitialized => initialized;

        abstract protected void Initialize(Game game, GraphicsDevice graphics, Matrix projectionMatrix, Matrix viewMatrix);
        abstract protected void updateGame(GameTime gameTime);
        abstract protected void updateGameInput();
        abstract public void drawGame(GraphicsDevice graphics);

        public void RunGame(Game game, GameTime gameTime, GraphicsDevice graphics, Matrix projectionMatrix, Matrix viewMatrix)
        {
            if (!initialized)
            {
                Initialize(game, graphics, projectionMatrix, viewMatrix);
                initialized = true;
            }
            updateGame(gameTime);
        }

        public static void DrawModel(Model model, Matrix modelTransform,
            Matrix[] absoluteBoneTransforms)
        {

            //Draw the model, a model can have multiple meshes so loop
            foreach (ModelMesh mesh in model.Meshes)
            {
                //This is where the mesh orientation is set
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World =
                        absoluteBoneTransforms[mesh.ParentBone.Index] *
                        modelTransform;
                }
                //Draw the mesh, will use the effects set above
                mesh.Draw();
            }
        }

        public Matrix[] SetupEffectDefaults(Model myModel, Matrix projectionMatrix, Matrix viewMatrix)
        {
            Matrix[] absoluteTransforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(absoluteTransforms);

            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.AmbientLightColor = new Vector3(1.0f, 1.0f, 1.0f);
                    effect.DirectionalLight0.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
                    effect.DirectionalLight1.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f);
                    effect.Projection = projectionMatrix;
                    effect.View = viewMatrix;
                }
            }
            return absoluteTransforms;
        }

    }
}
