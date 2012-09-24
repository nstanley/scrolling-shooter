﻿using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using ScrollingShooterWindowsLibrary;

namespace ScrollingShooterContentPipeline
{
    /// <summary>
    /// The content pipeline equivalent of a GameObjectGroup
    /// </summary>
    [ContentSerializerRuntimeType("ScrollingShooterWindowsLibrary.GameObjectGroup, ScrollingShooterWindowsLibrary")]
    public struct GameObjectGroupContent
    {
        /// <summary>
        /// The current scrolling offset in the y axis
        /// </summary>
        public float ScrollOffset;

        /// <summary>
        /// The speed at which this layer scrolls
        /// (needed for paralax scrolling)
        /// </summary>
        public float ScrollingSpeed;

        /// <summary>
        /// The depth of the objects in this layer
        /// (0 is foremost, and 1 is rearmost)
        /// </summary>
        public float LayerDepth;

        /// <summary>
        /// The group object data of this layer
        /// </summary>
        public GameObjectData[] GameObjectData;

        /// <summary>
        /// A list of properties to apply to this object group.  These
        /// should be converted to more meaningful and efficient
        /// variable representations in our TilemapProcessor
        /// </summary>
        [ContentSerializerIgnore]
        public Dictionary<string, string> Properties;
    }
}
