using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Magpie.Engine {
    public enum RESOURCE_TYPE {
        EFFECT, MODEL, TEXTURE, CUBEMAP, FONT, INV, RENDERTARGET, GBUFFER,
        AREAMAP
    }
    
    public class Resource {
        object _value;
        public object value {
            get {
                if (!loaded) {
                    Load();
                }

                return _value;
            }
            set {
                _value = value;
            }
        }

        public Texture2D value_tx => (Texture2D)value;
        public SpriteFont value_ft => (SpriteFont)value;
        public Effect value_fx => (Effect)value;
        public Model value_gfx => (Model)value;
        public RenderTarget2D value_rt => (RenderTarget2D)value;
        public Texture2D value_map => (Texture2D)value;
        public TextureCube value_cm => (TextureCube)value;
        public GBuffer value_gb => (GBuffer)value;

        public RESOURCE_TYPE resource_type { get; set; }

        private int _countdown_id = -1;
        public bool loaded { get; set; } = false;

        public string name { get; }
        public string fullname { get; } = "";
        private string getFullName() {
            string s = "";
            switch (resource_type) {
                case RESOURCE_TYPE.EFFECT: s = "sha"; break;
                case RESOURCE_TYPE.MODEL: s = "gfx"; break;
                case RESOURCE_TYPE.TEXTURE: s = "tex"; break;
                case RESOURCE_TYPE.FONT: s = "fnt"; break;
                case RESOURCE_TYPE.CUBEMAP: s = "cmp"; break;
                case RESOURCE_TYPE.AREAMAP: s = "map"; break;
            }

            return string.Format("{0}/{1}", s, name);
        }

        public Resource() {
        }

        public Resource(string name, RESOURCE_TYPE type) {
            this.name = name;
            resource_type = type;
            fullname = getFullName();
        }

        public Resource(string name, Texture2D texture) {
            this.name = name;
            resource_type = RESOURCE_TYPE.TEXTURE;
            fullname = getFullName();
            loaded = true;
            _value = texture;
        }

        public Resource(string name, Model model) {
            this.name = name;
            resource_type = RESOURCE_TYPE.MODEL;
            fullname = getFullName();
            loaded = true;
            _value = model;
        }

        public Resource(string name, RenderTarget2D rendertarget) {
            this.name = name;
            resource_type = RESOURCE_TYPE.RENDERTARGET;
            fullname = name;
            loaded = true;
            _value = rendertarget;
        }

        public Resource(string name, GBuffer gbuffer) {
            this.name = name;
            resource_type = RESOURCE_TYPE.GBUFFER;
            fullname = name;
            loaded = true;
            _value = gbuffer;
        }
        Matrix[] default_bone_transforms;

        public void Load() {
            if (!loaded) {
                switch (resource_type) {
                    case RESOURCE_TYPE.EFFECT: _value = ContentHandler.content.Load<Effect>(fullname); break;
                    case RESOURCE_TYPE.MODEL:
                        _value = ContentHandler.content.Load<Model>(fullname);
                        break;
                    case RESOURCE_TYPE.TEXTURE: _value = ContentHandler.content.Load<Texture2D>(fullname); break;
                    case RESOURCE_TYPE.CUBEMAP: _value = ContentHandler.content.Load<TextureCube>(fullname); break;
                    case RESOURCE_TYPE.FONT: _value = ContentHandler.content.Load<SpriteFont>(fullname); break;
                    case RESOURCE_TYPE.AREAMAP: _value = ContentHandler.content.Load<Texture2D>(fullname); break;
                }

                loaded = true;
            }
        }

        public void Load(ContentManager content) {
            if (!loaded) {
                switch (resource_type) {
                    case RESOURCE_TYPE.EFFECT: _value = content.Load<Effect>(fullname); break;
                    case RESOURCE_TYPE.MODEL:
                        _value = content.Load<Model>(fullname);
                        break;
                    case RESOURCE_TYPE.TEXTURE: _value = content.Load<Texture2D>(fullname); break;
                    case RESOURCE_TYPE.CUBEMAP: _value = content.Load<TextureCube>(fullname); break;
                    case RESOURCE_TYPE.FONT: _value = content.Load<SpriteFont>(fullname); break;
                    case RESOURCE_TYPE.AREAMAP: _value = content.Load<Texture2D>(fullname); break;
                }

                loaded = true;
            }
        }

        public void Unload() {
            if (loaded) {
                _value = null;
                loaded = false;
            }
        }
    }

    public class ContentHandlerSingleFN {
        Resource _resource;
        public Resource resource { get => _resource; set => _resource = value; }

        private ContentManager _content;        
        public ContentManager content { get => _content; set => _content = value; }

        public FileInfo file_info;
        public string file_name => file_info.FullName;
        public string short_name => file_name.Replace(".xnb", "");

        DirectoryInfo directory_info => file_info.Directory;
        string directory_name => directory_info.FullName;
        
        public ContentHandlerSingleFN(string filename, RESOURCE_TYPE type) {
            _content = new ContentManager(ContentHandler.content.ServiceProvider);
            file_info = new FileInfo(filename);

            _content.RootDirectory = file_info.DirectoryName;

            _resource = new Resource();
            _resource.resource_type = type;

            switch (type) {
                case RESOURCE_TYPE.EFFECT: _resource.value = content.Load<Effect>(short_name); break;
                case RESOURCE_TYPE.MODEL: _resource.value = content.Load<Model>(short_name); break;
                case RESOURCE_TYPE.TEXTURE: _resource.value = content.Load<Texture2D>(short_name); break;
                case RESOURCE_TYPE.CUBEMAP: _resource.value = content.Load<TextureCube>(short_name); break;
                case RESOURCE_TYPE.FONT: _resource.value = content.Load<SpriteFont>(short_name); break;
                case RESOURCE_TYPE.AREAMAP: _resource.value = content.Load<Texture2D>(short_name); break;
            }

            _resource.loaded = true;
        }        

        public void Unload() {
            resource.Unload();                        
            _content.Unload();            
        }
    }

    public class ContentHandler {
        static Texture2D _onePXWhite;
        static Texture2D _onePXBlack;
        static Texture2D _onePXGrey;
        static Texture2D _halfGrey;

        const bool use_error_texture = false;
        const bool use_error_model = false;

        private static Dictionary<string, Resource> _resources = new Dictionary<string, Resource>();

        private static ContentManager _content;

        public static Dictionary<string, Resource> resources { get => _resources; set => _resources = value; }
        public static ContentManager content { get => _content; set => _content = value; }

        static string _key;

        public static int count_textures => resources.Values.Where(a => a.resource_type == RESOURCE_TYPE.TEXTURE).Count();
        public static int count_models => resources.Values.Where(a => a.resource_type == RESOURCE_TYPE.MODEL).Count();
        public static int count_shaders => resources.Values.Where(a => a.resource_type == RESOURCE_TYPE.EFFECT).Count();
        public static int count_fonts => resources.Values.Where(a => a.resource_type == RESOURCE_TYPE.FONT).Count();
        public static int count_rts => resources.Values.Where(a => a.resource_type == RESOURCE_TYPE.RENDERTARGET).Count();
        public static int count_gbs => resources.Values.Where(a => a.resource_type == RESOURCE_TYPE.GBUFFER).Count();
        public static int count_cms => resources.Values.Where(a => a.resource_type == RESOURCE_TYPE.CUBEMAP).Count();
        public static int count_areas => resources.Values.Where(a => a.resource_type == RESOURCE_TYPE.AREAMAP).Count();

        public static void LoadAll() {
            foreach (Resource v in _resources.Values) {
                v.Load();
            }
        }

        public static void UnloadAll() {
            foreach (Resource v in _resources.Values) {
                v.Unload();
            }

            _content.Unload();
        }

        static bool pixel_loaded = false;

        public static void AddResource(Resource resource) {
            if (!resources.ContainsKey(resource.name))
                resources.Add(resource.name, resource);
            else
                Console.WriteLine("resource already added: " + resource.name);
        }

        public static void LoadContent(ContentManager Content, GraphicsDevice gd) {
            if (!pixel_loaded) {
                _onePXWhite = new Texture2D(gd, 1, 1);
                _onePXWhite.SetData<Color>(new Color[1] { Color.White });
                
                _onePXBlack = new Texture2D(gd, 1, 1);
                _onePXBlack.SetData<Color>(new Color[1] { Color.Black });

                _onePXGrey = new Texture2D(gd, 1, 1);
                _onePXGrey.SetData<Color>(new Color[1] { Color.FromNonPremultiplied(127,127,127, 255) });

                _halfGrey = new Texture2D(gd, 1, 1);
                _halfGrey.SetData<Color>(new Color[1] { Color.FromNonPremultiplied(127, 127, 127, 127) });

                _content = Content;

                AddResource(new Resource("OnePXWhite", _onePXWhite));
                AddResource(new Resource("OnePXBlack", _onePXBlack));
                AddResource(new Resource("OnePXGrey", _onePXGrey));
                AddResource(new Resource("HalfGrey", _halfGrey));

                AddResource(new Resource("center_glow", new Texture2D(gd, 1, 256)));
                AddResource(new Resource("gradient_vertical", new Texture2D(gd, 256, 256)));
                AddResource(new Resource("skybox_gradient", new Texture2D(gd, 512, 512)));

                var glowData = new Color[256];
                for (var i = 0; i < glowData.Length; i++) {
                    var p = i / (glowData.Length / 2f);
                    if (p > 1) p = 1f - (p - 1);

                    glowData[i] = Color.FromNonPremultiplied(255, 255, 255, (int)(p * 155));
                }

                ((Texture2D)resources["center_glow"].value).SetData(glowData);

                glowData = new Color[256 * 256];
                for (var i = 0; i < 255; i++) {
                    for (var x = 0; x < 255; x++) {
                        glowData[(i * 255) + x] = Color.FromNonPremultiplied(255, 255, 255, i);
                    }
                }

                ((Texture2D)resources["gradient_vertical"].value).SetData(glowData);

                glowData = new Color[512 * 512];
                for (var y = 0; y < 512; y++) {
                    for (var x = 0; x < 512; x++) {
                        float px = 1.0f - x / 512f; //x pos
                        float pxs = x / (512f / 2f); //x pos wave func, 0 to 1 to 0 at left/middle/right
                        if (pxs > 1) pxs = 1f - (pxs - 1);
                        float py = y / 512f; //y pos

                        int v = 128; //set entire image to 50%
                        v = (int)(v * (1 - (1 - (py / 2)))); //~50% black, 50% gradient fade from top to bottom
                        v += (y - (int)((512f / (MathHelper.Pi)) * (Math.Sin(px * MathHelper.Pi)))) / 4; //the actual curve
                        v = (int)(v - (((MathHelper.Clamp(1 - pxs, 0f, 1f)) * v) / 4f)); //reduce the brightness of the left and right edges slightly

                        if (v < 0) v = 0;

                        glowData[(y * 512) + x] = Color.FromNonPremultiplied(255, 255, 255,
                            v);
                    }
                }

                ((Texture2D)resources["skybox_gradient"].value).SetData(glowData);

                glowData = null;
                pixel_loaded = true;
            }

            DirectoryInfo di = new DirectoryInfo(Content.RootDirectory);

            if (di.Exists) {
                foreach (DirectoryInfo d in di.GetDirectories()) {
                    RESOURCE_TYPE type = RESOURCE_TYPE.INV;
                    switch (d.Name) {
                        case "fnt": type = RESOURCE_TYPE.FONT; break;
                        case "gfx": type = RESOURCE_TYPE.MODEL; break;
                        case "sha": type = RESOURCE_TYPE.EFFECT; break;
                        case "tex": type = RESOURCE_TYPE.TEXTURE; break;
                        case "cmp": type = RESOURCE_TYPE.CUBEMAP; break;
                        case "map": type = RESOURCE_TYPE.AREAMAP; break;
                    }

                    if (type == RESOURCE_TYPE.INV) continue;

                    foreach (FileInfo fi in d.GetFiles()) {
                        _key = fi.Name.Replace(fi.Extension, "");
                        if (_resources.ContainsKey(_key))
                            _resources.Remove(_key);

                        _resources.Add(_key, new Resource(fi.Name.Replace(fi.Extension, ""), type));
                    }
                }
            }
        }

        public static string[] ListTextures() {
            string[] tmp = new string[count_textures];
            int c = 0;

            foreach (Resource s in _resources.Values.Where(a => a.resource_type == RESOURCE_TYPE.TEXTURE)) {
                tmp[c] = s.name;

                c++;
            }
            return tmp;

        }

        public static bool Contains(string resource_name) {
            return _resources.ContainsKey(resource_name);
        }

        public static bool ContainsAndIsType(string resource_name, RESOURCE_TYPE type) {
            return _resources.ContainsKey(resource_name) && _resources[resource_name].resource_type == type;
        }       

    }
}
