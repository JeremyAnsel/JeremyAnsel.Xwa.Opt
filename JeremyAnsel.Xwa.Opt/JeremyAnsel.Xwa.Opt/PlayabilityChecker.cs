using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JeremyAnsel.Xwa.Opt
{
    public static class PlayabilityChecker
    {
        public static IList<PlayabilityMessage> CheckPlayability(OptFile opt)
        {
            var messages = new List<PlayabilityMessage>();

            messages.AddRange(PlayabilityChecker.OptInformations(opt));
            messages.AddRange(PlayabilityChecker.CheckTextures(opt));
            messages.AddRange(PlayabilityChecker.CheckGeometry(opt));
            messages.AddRange(PlayabilityChecker.CheckEngineGlows(opt));
            messages.AddRange(PlayabilityChecker.CheckHardpoints(opt));
            messages.AddRange(PlayabilityChecker.CheckFlatTextures(opt));

            messages.Sort();

            return messages;
        }

        public static IEnumerable<PlayabilityMessage> OptInformations(OptFile opt)
        {
            if (opt == null)
            {
                yield break;
            }

            yield return new PlayabilityMessage(
                PlayabilityMessageLevel.Information,
                "Opt",
                "FileName: {0}",
                opt.FileName);

            yield return new PlayabilityMessage(
                PlayabilityMessageLevel.Information,
                "Textures",
                "Textures Bits Per Pixel: {0}",
                opt.TexturesBitsPerPixel);
        }

        public static IEnumerable<PlayabilityMessage> CheckTextures(OptFile opt)
        {
            if (opt == null)
            {
                yield break;
            }

            if (opt.Textures.Count != 0 && opt.TexturesBitsPerPixel == 0)
            {
                yield return new PlayabilityMessage(
                    PlayabilityMessageLevel.Error,
                    "Textures",
                    "All textures have not the same bpp.");
            }

            if (opt.Textures.Count > 200)
            {
                yield return new PlayabilityMessage(
                    PlayabilityMessageLevel.Warning,
                    "Textures",
                    "This opt requires the textures count patch to work.");
            }

            if (opt.Textures.Values.Any(t => t.Width > 256 || t.Height > 256))
            {
                yield return new PlayabilityMessage(
                    PlayabilityMessageLevel.Warning,
                    "Textures",
                    "This opt requires the textures size patch to work.");
            }

            if (opt.TexturesBitsPerPixel == 32)
            {
                yield return new PlayabilityMessage(
                    PlayabilityMessageLevel.Warning,
                    "Textures",
                    "This opt requires the 32-bit textures patch to work.");
            }

            if (opt.TexturesBitsPerPixel == 32)
            {
                if (opt.Textures.Values.Any(t => t.MaximumMipmapsCount != t.MipmapsCount && t.MipmapsCount < 6))
                {
                    yield return new PlayabilityMessage(
                        PlayabilityMessageLevel.Error,
                        "Textures",
                        "Textures mipmaps have not been generated.");
                }
            }
        }

        public static IEnumerable<PlayabilityMessage> CheckGeometry(OptFile opt)
        {
            if (opt == null)
            {
                yield break;
            }

            if (opt.Meshes.Count > 50)
            {
                yield return new PlayabilityMessage(
                    PlayabilityMessageLevel.Error,
                    "Meshes",
                    "The maximum meshes count for an opt is 50.");
            }

            if (opt.Meshes.Any(t => t.Vertices.Count > 512))
            {
                yield return new PlayabilityMessage(
                    PlayabilityMessageLevel.Warning,
                    "Meshes",
                    "This opt requires the opt limit hook to work.");
            }

            if (opt.Meshes
                .SelectMany(t => t.Lods)
                .SelectMany(t => t.FaceGroups)
                .Where(t => t.Textures.Any(name =>
                {
                    Texture texture;
                    if (opt.Textures.TryGetValue(name, out texture))
                    {
                        return texture.HasAlpha;
                    }

                    return false;
                }))
                .Any(t => t.VerticesCount > 384))
            {
                yield return new PlayabilityMessage(
                    PlayabilityMessageLevel.Error,
                    "Meshes",
                    "The maximum vertices count for a face group is 384. Save the opt to automatically split the face groups.");
            }
        }

        public static IEnumerable<PlayabilityMessage> CheckEngineGlows(OptFile opt)
        {
            if (opt == null)
            {
                yield break;
            }

            if (opt.EngineGlowsCount > 16)
            {
                yield return new PlayabilityMessage(
                    PlayabilityMessageLevel.Error,
                    "Engine Glows",
                    "The maximum engine glows count for an opt is 16.");
            }
        }

        public static IEnumerable<PlayabilityMessage> CheckHardpoints(OptFile opt)
        {
            if (opt == null)
            {
                yield break;
            }

            if (opt.HardpointsCount > 256)
            {
                yield return new PlayabilityMessage(
                    PlayabilityMessageLevel.Error,
                    "Hardpoints",
                    "The maximum hardpoints count for an opt is 256.");
            }
            else if (opt.HardpointsCount > 128)
            {
                yield return new PlayabilityMessage(
                    PlayabilityMessageLevel.Warning,
                    "Hardpoints",
                    "The maximum hardpoints count for an opt is 128 if hardpoints mirroring is enabled for this opt.");
            }

            if (opt.Meshes.SelectMany(t => t.Hardpoints).Count(t => t.HardpointType == HardpointType.CockpitSparks) > 16)
            {
                yield return new PlayabilityMessage(
                    PlayabilityMessageLevel.Error,
                    "Hardpoints",
                    "The maximun cockpit sparks count for an opt is 16.");
            }

            if (opt.Meshes.SelectMany(t => t.Hardpoints).Count(t => t.HardpointType == HardpointType.AccEnd || t.HardpointType == HardpointType.Gunner) > 8)
            {
                yield return new PlayabilityMessage(
                    PlayabilityMessageLevel.Error,
                    "Hardpoints",
                    "The maximun acc ends count for an opt is 8.");
            }

            if (opt.Meshes.SelectMany(t => t.Hardpoints).Count(t => t.HardpointType == HardpointType.DockingPoint) > 9)
            {
                yield return new PlayabilityMessage(
                    PlayabilityMessageLevel.Error,
                    "Hardpoints",
                    "The maximun docking points count for an opt is 9.");
            }

            if (opt.Meshes.SelectMany(t => t.Hardpoints).Count(t => t.HardpointType == HardpointType.JammingPoint) > 8)
            {
                yield return new PlayabilityMessage(
                    PlayabilityMessageLevel.Error,
                    "Hardpoints",
                    "The maximun jamming points count for an opt is 8.");
            }
        }

        public static IEnumerable<PlayabilityMessage> CheckFlatTextures(OptFile opt)
        {
            if (opt == null)
            {
                yield break;
            }

            var flatTextures = opt.CheckFlatTextures(false);

            foreach(string texture in flatTextures)
            {
                yield return new PlayabilityMessage(
                    PlayabilityMessageLevel.Warning,
                    "Flat Textures",
                    texture);
            }
        }
    }
}
