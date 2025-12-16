using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Mediator.DTO.API
{
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// User-friendly message about the result
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Detailed error information (only populated on failure)
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// The data payload of the response
        /// </summary>
        public T? Data { get; set; }
    }
}
