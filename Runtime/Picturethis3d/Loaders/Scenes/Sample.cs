using UnityEngine;

namespace Picturethis3d.Loaders
{
    public class Sample : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            SpriteLoaders.Clear();
            TextureLoaders.Clear();
            GameObjectLoaders.Clear();
        }
    }
}