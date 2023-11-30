//Models/article.cs

using System.ComponentModel.DataAnnotations;

public class Article {

    [Required(ErrorMessage = "A code is required")]
    [StringLength(13, MinimumLength = 1, ErrorMessage = "A Code must be between 1 and 13 characters")]
    public string Code { get; set; }

    [Required(ErrorMessage = "A name is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "A Name must be between 1 and 50 characters")]
    public string Name { get; set; }

    [Required(ErrorMessage = "A numerical pot size is required")]
    public int PotSize { get; set; }

    [Required(ErrorMessage = "A numerical plant height is required")]
    public int PlantHeight { get; set; }

    [Required(ErrorMessage = "A product group is required")]
    public string ProductGroup { get; set;}

    //Optional
    public string Colour { get; set;}

}
