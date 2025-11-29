// Controllers/UploadsController.cs
using Microsoft.AspNetCore.Mvc;
using backend_shopcaulong.Services;

namespace backend_shopcaulong.Controllers
{
    [Route("api/test-upload")]
    [ApiController]
    public class UploadsController : ControllerBase
    {
        private readonly IUploadService _uploadService;

        public UploadsController(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        /// <summary>
        /// 1. TEST UPLOAD ẢNH CHÍNH SẢN PHẨM (gallery - nhiều ảnh)
        /// POST /api/test-upload/main-images
        /// form-data: key = "files" → chọn nhiều ảnh
        /// </summary>
        [HttpPost("main-images")]
        public async Task<IActionResult> TestUploadMainImages(IFormFileCollection files)
        {
            if (files == null || files.Count == 0)
                return BadRequest(new { message = "Vui lòng chọn ít nhất 1 file!" });

            try
            {
                var urls = await _uploadService.UploadProductImagesAsync(files);
                return Ok(new
                {
                    message = $"Upload thành công {urls.Count} ảnh chính sản phẩm!",
                    count = urls.Count,
                    urls
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi upload ảnh chính", error = ex.Message });
            }
        }

        /// <summary>
        /// 2. TEST UPLOAD ẢNH CHO MỘT MÀU (Color Variant) – RẤT QUAN TRỌNG!
        /// POST /api/test-upload/color-images
        /// form-data: key = "files" → chọn nhiều ảnh cho màu Đỏ, Xanh, Trắng...
        /// → Đây chính là ảnh bạn sẽ dùng cho từng ColorVariant
        /// </summary>
        [HttpPost("color-images")]
        public async Task<IActionResult> TestUploadColorImages(IFormFileCollection files)
        {
            if (files == null || files.Count == 0)
                return BadRequest(new { message = "Vui lòng chọn ít nhất 1 ảnh cho màu này!" });

            try
            {
                var urls = await _uploadService.UploadProductImagesAsync(files);
                return Ok(new
                {
                    message = $"Upload thành công {urls.Count} ảnh cho một màu (Color Variant)!",
                    color_tip = "Dùng các URL này trong ColorVariantCreateDto.ImageFiles khi tạo/sửa sản phẩm",
                    count = urls.Count,
                    urls
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi upload ảnh màu", error = ex.Message });
            }
        }

        /// <summary>
        /// 3. TEST UPLOAD ẢNH CHI TIẾT MÔ TẢ (ProductDetail)
        /// POST /api/test-upload/detail-image
        /// form-data: key = "file" → chọn 1 ảnh
        /// </summary>
        [HttpPost("detail-image")]
        public async Task<IActionResult> TestUploadDetailImage(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn 1 file ảnh chi tiết!" });

            try
            {
                var url = await _uploadService.UploadDetailImageAsync(file);
                return Ok(new
                {
                    message = "Upload ảnh chi tiết mô tả thành công!",
                    url,
                    tip = "Dùng URL này trong ProductDetailCreateDto.ImageFile"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi upload ảnh chi tiết", error = ex.Message });
            }
        }

        /// <summary>
        /// 4. XÓA FILE TEST (nếu cần dọn rác)
        /// POST /api/test-upload/delete
        /// body: { "url": "/images/products/abc123.jpg" }
        /// </summary>
        [HttpPost("delete")]
        public IActionResult TestDeleteFile([FromBody] DeleteFileRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Url))
                return BadRequest(new { message = "Vui lòng cung cấp URL ảnh cần xóa!" });

            try
            {
                _uploadService.DeleteFile(request.Url);
                return Ok(new { message = "Đã xóa file thành công!", deleted_url = request.Url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa file", error = ex.Message });
            }
        }

        /// <summary>
        /// 5. Xem tất cả ảnh đã upload – để kiểm tra bằng mắt
        /// GET /api/test-upload/sample
        /// </summary>
        [HttpGet("sample")]
        public IActionResult GetSampleLinks()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            return Ok(new
            {
                message = "Mở các link dưới đây để xem ảnh đã upload (thư mục chung)",
                lưu_ý_quan_trọng = "Tất cả ảnh sản phẩm + ảnh màu đều lưu chung trong /images/products/",
                ảnh_chính_và_ảnh_màu = $"{baseUrl}/images/products/",
                ảnh_chi_tiết_mô_tả = $"{baseUrl}/images/products/details/",
                tip = "Không còn thư mục /variants nữa vì giờ ảnh màu dùng chung folder products!",
                swagger_test = "Dùng các endpoint trên để upload → refresh link → thấy ảnh = thành công!"
            });
        }
    }

    // DTO nhỏ để test xóa file
    public class DeleteFileRequest
    {
        public string? Url { get; set; }
    }
}