using Common.Models.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Planner
{
    public class GalleryImage
    {
        [Key]
        public Guid Id { get; set; }

        public Guid GalleryAlbumId { get; set; }
        public GalleryCategory Category { get; set; }
        [Required] public string ImagePath { get; set; } = default!;

        [ForeignKey(nameof(GalleryAlbumId))]
        public virtual GalleryAlbum GalleryAlbum { get; set; } = default!;
    }
}
