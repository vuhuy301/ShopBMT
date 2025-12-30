import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import styles from "./UpdateProductPage.module.css";
import { updateProduct } from "../../services/admin/productAdminService";
import { getProductById } from "../../services/productService";
import { getCategories } from "../../services/categoryService";
import { getBrands } from "../../services/brandService";

const IMAGE_BASE = process.env.REACT_APP_IMAGE_BASE_URL;

// ✅ Helper để tạo unique ID tránh trùng lặp
let uniqueCounter = 0;
const generateUniqueId = () => {
  return `temp_${Date.now()}_${++uniqueCounter}`;
};

const ProductUpdatePage = () => {
  const { id } = useParams();
  const [product, setProduct] = useState(null);
  const [blobUrls, setBlobUrls] = useState([]);

  const [categories, setCategories] = useState([]);
  const [brands, setBrands] = useState([]);

  useEffect(() => {
    const load = async () => {
      const data = await getProductById(id);
      // Đảm bảo có mảng rỗng nếu null
      data.images = data.images || [];
      data.details = data.details || [];
      data.colorVariants = data.colorVariants || [];
      setProduct(data);
    };
    load();
  }, [id]);

  useEffect(() => {
    return () => blobUrls.forEach(url => URL.revokeObjectURL(url));
  }, [blobUrls]);

  useEffect(() => {
    const loadDropdown = async () => {
      const cats = await getCategories();
      const brs = await getBrands();
      setCategories(cats);
      setBrands(brs);
    };
    loadDropdown();
  }, []);

  const createBlob = (file) => {
    const url = URL.createObjectURL(file);
    setBlobUrls(prev => [...prev, url]);
    return url;
  };

  if (!product) return <div>Loading...</div>;

  const updateField = (field, value) => {
    setProduct(prev => ({ ...prev, [field]: value }));
  };

  // ==================== MAIN IMAGES ====================
  const addMainImage = () => {
    setProduct(prev => ({
      ...prev,
      images: [...prev.images, { id: null, imageUrl: "", isPrimary: false }]
    }));
  };

  const handleMainImageChange = (e, index) => {
    const file = e.target.files[0];
    if (!file) return;
    const url = createBlob(file);
    setProduct(prev => {
      const imgs = [...prev.images];
      imgs[index] = { ...imgs[index], imageUrl: url, _file: file };
      return { ...prev, images: imgs };
    });
  };

  const removeMainImage = (index) => {
    setProduct(prev => ({
      ...prev,
      images: prev.images.filter((_, i) => i !== index)
    }));
  };

  const setPrimary = (index) => {
    setProduct(prev => ({
      ...prev,
      images: prev.images.map((img, i) => ({ ...img, isPrimary: i === index }))
    }));
  };

  // ==================== DETAILS ====================
  const addDetail = () => {
    setProduct(prev => ({
      ...prev,
      details: [...prev.details, { 
        id: generateUniqueId(), 
        text: "", 
        imageUrl: "", 
        sortOrder: prev.details.length 
      }]
    }));
  };

  const updateDetailField = (index, field, value) => {
    setProduct(prev => {
      const d = [...prev.details];
      d[index] = { ...d[index], [field]: value };
      return { ...prev, details: d };
    });
  };

  const handleDetailImage = (e, index) => {
    const file = e.target.files[0];
    if (!file) return;
    const url = createBlob(file);
    setProduct(prev => {
      const d = [...prev.details];
      d[index] = { ...d[index], imageUrl: url, _file: file };
      return { ...prev, details: d };
    });
  };

  const removeDetail = (index) => {
    setProduct(prev => ({
      ...prev,
      details: prev.details.filter((_, i) => i !== index)
    }));
  };

  // ==================== COLOR VARIANTS ====================
  const addColorVariant = () => {
    setProduct(prev => ({
      ...prev,
      colorVariants: [...prev.colorVariants, {
        id: generateUniqueId(),
        color: "",
        imageUrls: [],
        sizes: []
      }]
    }));
  };

  const updateColorField = (index, field, value) => {
    setProduct(prev => {
      const cv = [...prev.colorVariants];
      cv[index] = { ...cv[index], [field]: value };
      return { ...prev, colorVariants: cv };
    });
  };

  const handleColorImages = (e, variantIndex) => {
    const files = Array.from(e.target.files);
    if (files.length === 0) return;
    const urls = files.map(f => createBlob(f));
    setProduct(prev => {
      const cv = [...prev.colorVariants];
      cv[variantIndex] = {
        ...cv[variantIndex],
        imageUrls: [...cv[variantIndex].imageUrls, ...urls],
        _files: [...(cv[variantIndex]._files || []), ...files]
      };
      return { ...prev, colorVariants: cv };
    });
  };

  const removeColorImage = (variantIndex, imgIndex) => {
    setProduct(prev => {
      const cv = [...prev.colorVariants];
      const variant = { ...cv[variantIndex] };
      
      // ✅ Tạo bản sao mới thay vì mutate trực tiếp
      variant.imageUrls = variant.imageUrls.filter((_, i) => i !== imgIndex);
      if (variant._files) {
        variant._files = variant._files.filter((_, i) => i !== imgIndex);
      }
      
      cv[variantIndex] = variant;
      return { ...prev, colorVariants: cv };
    });
  };

  // ✅ FIX: Sizes - tạo bản sao đúng cách, dùng unique ID
  const addSize = (vIdx) => {
    setProduct(prev => {
      const cv = [...prev.colorVariants];
      const variant = { ...cv[vIdx] };
      
      // Tạo size mới với ID unique
      const newSize = { 
        id: generateUniqueId(), 
        size: "", 
        stock: 0, 
        inStock: true 
      };
      
      // Tạo mảng sizes mới
      variant.sizes = [...variant.sizes, newSize];
      cv[vIdx] = variant;
      
      return { ...prev, colorVariants: cv };
    });
  };

  const updateSize = (vIdx, sIdx, field, value) => {
    setProduct(prev => {
      const cv = [...prev.colorVariants];
      const variant = { ...cv[vIdx] };
      const sizes = [...variant.sizes];
      
      sizes[sIdx] = { ...sizes[sIdx], [field]: value };
      variant.sizes = sizes;
      cv[vIdx] = variant;
      
      return { ...prev, colorVariants: cv };
    });
  };

  const removeSize = (vIdx, sIdx) => {
    setProduct(prev => {
      const cv = [...prev.colorVariants];
      const variant = { ...cv[vIdx] };
      
      variant.sizes = variant.sizes.filter((_, i) => i !== sIdx);
      cv[vIdx] = variant;
      
      return { ...prev, colorVariants: cv };
    });
  };

  const removeColorVariant = (index) => {
    setProduct(prev => ({
      ...prev,
      colorVariants: prev.colorVariants.filter((_, i) => i !== index)
    }));
  };

  // ==================== SUBMIT ====================
  const handleSubmit = async () => {
    console.log("Product before submit:", product);
    const formData = new FormData();

    // Thông tin cơ bản
    formData.append("Name", product.name || "");
    formData.append("Description", product.description || "");
    formData.append("Price", product.price || 0);
    formData.append("DiscountPrice", product.discountPrice || 0);
    formData.append("BrandId", product.brandId || 0);
    formData.append("CategoryId", product.categoryId || 0);
    formData.append("IsFeatured", product.isFeatured || false);

    // Ảnh chính cũ (giữ lại URL)
    product.images.forEach(img => {
      if (img.imageUrl && !img.imageUrl.startsWith("blob:")) {
        formData.append("ImageUrls", img.imageUrl);
      }
    });

    // Ảnh chính mới
    product.images.forEach(img => {
      if (img._file) {
        formData.append("ImageFiles", img._file);
      }
    });

    // Details
    product.details.forEach((d, i) => {
      // ✅ Chỉ gửi id nếu nó là số, không gửi string id tạm
      const detailId = typeof d.id === 'number' ? d.id : 0;
      formData.append(`Details[${i}].Id`, detailId);
      formData.append(`Details[${i}].Text`, d.text || "");
      formData.append(`Details[${i}].SortOrder`, d.sortOrder || 0);
      if (d.imageUrl && !d.imageUrl.startsWith("blob:")) {
        formData.append(`Details[${i}].ImageUrl`, d.imageUrl);
      }
      if (d._file) {
        formData.append(`Details[${i}].ImageFile`, d._file);
      }
    });

    // Color Variants
    product.colorVariants.forEach((cv, i) => {
      // ✅ Chỉ gửi id nếu nó là số
      const variantId = typeof cv.id === 'number' ? cv.id : 0;
      formData.append(`ColorVariants[${i}].Id`, variantId);
      formData.append(`ColorVariants[${i}].Color`, cv.color || "");

      // Ảnh cũ của color
      cv.imageUrls.forEach(url => {
        if (!url.startsWith("blob:")) {
          formData.append(`ColorVariants[${i}].ImageUrls`, url);
        }
      });

      // Ảnh mới của color
      if (cv._files) {
        cv._files.forEach(file => {
          formData.append(`ColorVariants[${i}].ImageFiles`, file);
        });
      }

      // Sizes
      cv.sizes.forEach((s, j) => {
        // ✅ Chỉ gửi id nếu nó là số
        const sizeId = typeof s.id === 'number' ? s.id : 0;
        formData.append(`ColorVariants[${i}].Sizes[${j}].Id`, sizeId);
        formData.append(`ColorVariants[${i}].Sizes[${j}].Size`, s.size || "");
        formData.append(`ColorVariants[${i}].Sizes[${j}].Stock`, s.stock || 0);
        formData.append(`ColorVariants[${i}].Sizes[${j}].InStock`, true);
      });
    });

    try {
      await updateProduct(id, formData);
      alert("Cập nhật sản phẩm thành công!");
    } catch (err) {
      console.error("Update error:", err);
      alert("Lỗi cập nhật: " + (err.response?.data?.title || err.message));
    }
  };

  return (
    <div className={styles.container}>
      <h2 className={styles.title}>Cập nhật sản phẩm</h2>

      {/* BASIC */}
      <div className={styles.section}>
        <label>Tên sản phẩm</label>
        <input
          value={product.name || ""}
          onChange={(e) => updateField("name", e.target.value)}
        />

        <label>Mô tả ngắn gọn</label>
        <textarea
          value={product.description || ""}
          onChange={(e) => updateField("description", e.target.value)}
        />

        <label>Giá gốc</label>
        <input
          type="number"
          value={product.price || ""}
          onChange={(e) => updateField("price", Number(e.target.value))}
        />

        <label>Giá khuyến mãi</label>
        <input
          type="number"
          value={product.discountPrice || ""}
          onChange={(e) => updateField("discountPrice", Number(e.target.value))}
        />

        <label>Danh mục sản phẩm</label>
        <select
          value={product.categoryId || ""}
          onChange={(e) => updateField("categoryId", Number(e.target.value))}
        >
          <option value="">-- Chọn danh mục --</option>
          {categories.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
            </option>
          ))}
        </select>

        <label>Thương hiệu</label>
        <select
          value={product.brandId || ""}
          onChange={(e) => updateField("brandId", Number(e.target.value))}
        >
          <option value="">-- Chọn thương hiệu --</option>
          {brands.map((b) => (
            <option key={b.id} value={b.id}>
              {b.name}
            </option>
          ))}
        </select>
      </div>

      {/* MAIN IMAGES */}
      <div className={styles.section}>
        <h3>Ảnh</h3>
        {product.images.map((img, i) => (
          <div key={i} className={styles.row}>
            <img
              src={img.imageUrl.startsWith("blob:") ? img.imageUrl : IMAGE_BASE + img.imageUrl}
              alt=""
              width={80}
              height={80}
              style={{ borderRadius: 8, objectFit: "cover" }}
            />
            <input type="file" onChange={e => handleMainImageChange(e, i)} />
            <label>
              <input type="checkbox" checked={!!img.isPrimary} onChange={() => setPrimary(i)} />
              Ảnh chính
            </label>
            <button onClick={() => removeMainImage(i)}>Xóa</button>
          </div>
        ))}
        <button onClick={addMainImage}>+ Thêm ảnh</button>
      </div>

      {/* DETAILS */}
      <div className={styles.section}>
        <h3>Mô tả chi tiết</h3>
        {product.details.map((d, i) => (
          <div key={d.id} className={styles.detailBox}>
            <textarea
              placeholder="Text"
              value={d.text || ""}
              onChange={e => updateDetailField(i, "text", e.target.value)}
            />
            {d.imageUrl && (
              <img
                src={d.imageUrl.startsWith("blob:") ? d.imageUrl : IMAGE_BASE + d.imageUrl}
                alt=""
                width={100}
                height={100}
                style={{ borderRadius: 6, marginTop: 8 }}
              />
            )}
            <input type="file" onChange={e => handleDetailImage(e, i)} />
            <input
              type="number"
              placeholder="Sort"
              value={d.sortOrder || ""}
              onChange={e => updateDetailField(i, "sortOrder", Number(e.target.value))}
            />
            <button onClick={() => removeDetail(i)}>Xóa</button>
          </div>
        ))}
        <button onClick={addDetail}>+ Thêm mô tả</button>
      </div>

      {/* COLOR VARIANTS */}
      <div className={styles.section}>
        <h3>Biến thể màu</h3>
        {product.colorVariants.map((cv, i) => (
          <div key={cv.id} className={styles.colorBox}>
            <input
              value={cv.color || ""}
              onChange={e => updateColorField(i, "color", e.target.value)}
              placeholder="Tên màu"
            />

            <h4>Ảnh của màu</h4>
            <div className={styles.imageList}>
              {cv.imageUrls.map((url, j) => (
                <div key={j} className={styles.imgWrap}>
                  <img
                    src={url.startsWith("blob:") ? url : IMAGE_BASE + url}
                    alt=""
                    width={100}
                    height={100}
                    style={{ borderRadius: 6 }}
                  />
                  <button className={styles.deleteBtn} onClick={() => removeColorImage(i, j)}>X</button>
                </div>
              ))}
            </div>
            <input type="file" multiple onChange={e => handleColorImages(e, i)} />

            <h4>Biến thể size</h4>
            {cv.sizes.map((s, j) => (
              <div key={s.id} className={styles.row}>
                <input 
                  value={s.size || ""} 
                  onChange={e => updateSize(i, j, "size", e.target.value)} 
                  placeholder="Size" 
                />
                <input 
                  type="number" 
                  value={s.stock || ""} 
                  onChange={e => updateSize(i, j, "stock", Number(e.target.value))} 
                  placeholder="Stock" 
                  style={{ width: '120px', minWidth: '120px' }}
                />
                <button onClick={() => removeSize(i, j)}>Xóa</button>
              </div>
            ))}
            <button onClick={() => addSize(i)}>+ Thêm size</button>
            <button onClick={() => removeColorVariant(i)}>Xóa màu</button>
          </div>
        ))}
        <button onClick={addColorVariant}>+ Thêm biến thể màu</button>
      </div>

      <button onClick={handleSubmit} className={styles.submitBtn}>
        Cập nhật sản phẩm
      </button>
    </div>
  );
};

export default ProductUpdatePage;