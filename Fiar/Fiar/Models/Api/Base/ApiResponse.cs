namespace Fiar
{
    /// <summary>
    /// The response for all Web API calls made
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResponse
    {
        #region Public Properties

        /// <summary>
        /// Indicates if the API call was successful
        /// </summary>
        public bool Successful => ErrorMessage == null;

        /// <summary>
        /// Should we raise authorization error indicator?
        /// TODO:LATER: Make the set only during construction
        ///       But it requires to have public get; set;
        /// <para>
        ///     By default, if anyone calls API without authorization, 
        ///     it will response with 401, but if there is authorization 
        ///     which is not valid and it fails afterwards, 
        ///     we need to have an indication about that.
        /// </para>
        /// </summary>
        public bool HasUnauthorizedUserError { get; set; }

        /// <summary>
        /// The error message for a failed API call
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The API response object
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public object Response { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ApiResponse()
        {
        }

        #endregion
    }

    /// <summary>
    /// The response for all Web API calls made, with a >specific type of known response
    /// </summary>
    /// <typeparam name="T">The specific type of server response</typeparam>
    public class ApiResponse<T> : ApiResponse
    {
        /// <summary>
        /// The API response object as T
        /// </summary>
        public new T Response { get => (T)base.Response; set => base.Response = value; }
    }
}
