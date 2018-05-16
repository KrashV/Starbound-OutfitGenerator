using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace OutfitGenerator_dotnetcore
{
    class ResourceManager
    {
        /// <summary>
        /// Gets the image streams from the image name
        /// </summary>
        /// <param name="name">Name of the image resouce</param>
        /// <returns>Image memory stream</returns>
        public static Stream GetResourceImage(string name)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            return _assembly.GetManifestResourceStream("OutfitGenerator_dotnetcore." + name);
        }

        /// <summary>
        /// Parses text bytes, and returns it as a string.
        /// Can be used for <see cref="Properties.Resources"/> text resources.
        /// </summary>
        /// <param name="resource">Text resource bytes.</param>
        /// <returns>Text resource.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DecoderFallbackException"></exception>
        public static string TextResource(byte[] resource)
        {
            return Encoding.Default.GetString(resource);
        }

        /// <summary>
        /// Fetches a text resource from the assemby.
        /// </summary>
        /// <param name="assembly">Assembly the resource resides in.</param>
        /// <param name="name">Name/Assembly path of the resource.</param>
        /// <returns></returns>
        public static string GetResourceText(string name)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            using (Stream resource = _assembly.GetManifestResourceStream("OutfitGenerator_dotnetcore." + name))
            {
                byte[] resBytes = new byte[resource.Length];
                resource.Read(resBytes, 0, resBytes.Length);
                return TextResource(resBytes);
            }
        }
    }
}
