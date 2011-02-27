﻿namespace Nancy.ViewEngines
{
    using System;
    using System.IO;

    /// <summary>
    /// Defines the functionality used by a <see cref="NancyModule"/> to render a view to the response.
    /// </summary>
    public interface IViewFactory : IHideObjectMembers
    {
        /// <summary>
        /// Renders the view with its name resolved from the model type, and model defined by the <paramref name="model"/> parameter.
        /// </summary>
        /// <param name="model">The model that should be passed into the view.</param>
        /// <returns>A delegate that can be invoked with the <see cref="Stream"/> that the view should be rendered to.</returns>
        /// <remarks>The view name is model.GetType().Name with any Model suffix removed.</remarks>
        Action<Stream> this[dynamic model] { get; }

        /// <summary>
        /// Renders the view with the name defined by the <paramref name="viewName"/> parameter.
        /// </summary>
        /// <param name="viewName">The name of the view to render.</param>
        /// <returns>A delegate that can be invoked with the <see cref="Stream"/> that the view should be rendered to.</returns>
        Action<Stream> this[string viewName] { get; }

        /// <summary>
        /// Renders the view with the name and model defined by the <paramref name="viewName"/> and <paramref name="model"/> parameters.
        /// </summary>
        /// <param name="viewName">The name of the view to render.</param>
        /// <param name="model">The model that should be passed into the view.</param>
        /// <returns>A delegate that can be invoked with the <see cref="Stream"/> that the view should be rendered to.</returns>
        Action<Stream> this[string viewName, dynamic model] { get; }
    }
}