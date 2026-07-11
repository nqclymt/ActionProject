using System;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

namespace PKC.ActionEditor
{
     /// <summary>
        /// 类排序
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class OrderAttribute : Attribute
        {
            public int Order;

            public OrderAttribute(int order)
            {
                Order = order;
            }
        }
        
        /// <summary>
        /// 菜单自定义名称
        /// </summary>
        [AttributeUsage(AttributeTargets.All, Inherited = false)]
        public class MenuNameAttribute: Attribute
        {
            public string MenuName;

            public MenuNameAttribute(string menuName)
            {
                MenuName = menuName;
            }
        }
        
        /// <summary>
        /// 关联某个类型
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class OptionParamAttribute: Attribute
        {
            public Type ClassType;

            public OptionParamAttribute(Type classType)
            {
                ClassType = classType;
            }
        }
        
        /// <summary>
        /// 选项排序
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class OptionSortAttribute:Attribute
        {
            public int sort;

            public OptionSortAttribute(int sort)
            {
                this.sort = sort;
            }
        }
        
        /// <summary>
        /// 关联某个字段或某个值
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class OptionRelateParamAttribute:Attribute
        {
            public string argsName;
            
            public object[] argsValues;

            public OptionRelateParamAttribute(string name, params object[] values)
            {
                this.argsName = name;
                argsValues = values;
            }
        }
        
        /// <summary>
        /// 选择对象路径
        /// </summary>
        public class SelectObjectPathAttribute:Attribute
        {
            public Type type;

            public SelectObjectPathAttribute(Type type)
            {
                this.type = type;
            }
        }
        
        /// <summary>
        /// 自定义检视面板
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class CustomInspectors:Attribute
        {
            public Type InspectedType;
            public bool _editorForChildClasses;

            public CustomInspectors(Type inspectedType)
            {
                this.InspectedType = inspectedType;
            }

            public CustomInspectors(Type inspectedType, bool editorForChildClasses)
            {
                InspectedType = inspectedType;
                _editorForChildClasses = editorForChildClasses;
            }
        }
        
        /// <summary>
        /// 自定义检视面板
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class CustomHeader:Attribute
        {
            public Type InspectedType;

            public CustomHeader(Type inspectorType)
            {
                InspectedType = inspectorType;
            }
            
        }
        
        /// <summary>
        /// 自定义名称
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class NameAttribute:Attribute
        {
            public readonly string name;

            public NameAttribute(string name)
            {
                this.name = name;
            }

        }

        /// <summary>
        /// 指定类别
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class CategoryAttribute : Attribute
        {
            public readonly string category;

            public CategoryAttribute(string category)
            {
                this.category = category;
            }
        }

        /// <summary>
        /// 指定描述
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class DescriptionAttribute : Attribute
        {
            public readonly string description;

            public DescriptionAttribute(string description)
            {
                this.description = description;
            }
        }

        /// <summary>
        /// 指定类型的图标
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class ShowIconAttribute : Attribute
        {
            public readonly string iconPath;

            public readonly Type fromType;
            
            public readonly Texture2D texture;

            public ShowIconAttribute(Texture2D texture)
            {
                this.iconPath = texture.name;
            }

            public ShowIconAttribute(string iconPath)
            {
                this.iconPath = iconPath;
            }

            public ShowIconAttribute(Type fromType)
            {
                this.fromType = fromType;
            }
        }

        /// <summary>
        /// 指定显示颜色
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class ColorAttribute : Attribute
        {
            public readonly Color Color;

            public ColorAttribute(float r, float g, float b, float a)
            {
                this.Color = new Color(r, g, b, a);
            }

            public ColorAttribute(Color color)
            {   
                this.Color = color;
            }
        }

        /// <summary>
        /// 指定附加类型
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class AttachableAttribute : Attribute
        {
            public readonly Type[] Types;

            public AttachableAttribute(params Type[] types)
            {
                this.Types = types;
            }

        }

        /// <summary>
        /// 组内唯一性
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class UniqueAttribute : Attribute
        {
            
        }

        /// <summary>
        /// 自定义片段预览
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class CustomPreviewAttribute : Attribute
        {
            public Type PreviewType;

            public CustomPreviewAttribute(Type previewType)
            {
                PreviewType = previewType;
            }
        }
}