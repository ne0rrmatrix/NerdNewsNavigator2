// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Model
{
    /// <summary>
    /// <c>Show</c> is a Class for storing Shows
    /// </summary>
    public class Show
    {
        /// <summary>
        /// The <c>Title</c> is a <see cref="string"/> of <see cref="Show"/> Class
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The <c>Description</c> is a <see cref="string"/> of <see cref="Show"/> Class.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The <c>Link</c> is a <see cref="string"/> of <see cref="Show"/> Class.
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// The <c>Image</c> is a <see cref="string"/> of <see cref="Show"/> Class.
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// The <c>Url</c> is a <see cref="string"/> of <see cref="Show"/> Class.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The <c>PubDate</c> is a <see cref="DateTime"/> of <see cref="Podcast"/> Class.
        /// </summary>
        public DateTime PubDate { get; set; }
    }
}
