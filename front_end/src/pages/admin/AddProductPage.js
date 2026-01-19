import React, { useState, useEffect } from "react";
import styles from "./AddProductPage.module.css";
import Select from "react-select";
import { useNavigate } from "react-router-dom";

import { createProduct } from "../../services/admin/productAdminService";
import { getCategories } from "../../services/categoryService";
import { getBrands } from "../../services/brandService";
import { validateProduct } from "../../utils/addProductValidation";
import { getAllPromotions } from "../../services/admin/promotionService";


const AddProductPage = () => {
    const navigate = useNavigate();
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [price, setPrice] = useState("");
    const [discountPrice, setDiscountPrice] = useState("");
    const [brandId, setBrandId] = useState("");
    const [categoryId, setCategoryId] = useState("");
    const [isFeatured, setIsFeatured] = useState(false);

    const [categories, setCategories] = useState([]);
    const [brands, setBrands] = useState([]);

    // Ảnh chính
    const [mainImages, setMainImages] = useState([]);

    // Chi tiết mô tả
    const [details, setDetails] = useState([
        { text: "", sortOrder: 0, imageFile: null, preview: null }
    ]);

    // Biến thể màu
    const [colorVariants, setColorVariants] = useState([
        {
            color: "",
            imageFiles: [],
            sizes: [{ size: "", stock: 0 }]
        }
    ]);

    const [errors, setErrors] = useState({});

const [promotions, setPromotions] = useState([]);
const [selectedPromotions, setSelectedPromotions] = useState([]);


    // ============================
    // LOAD CATEGORY + BRAND
    // ============================
    useEffect(() => {
        const fetchData = async () => {
            try {
                const cats = await getCategories();
                const brs = await getBrands();
                setCategories(cats);
                setBrands(brs);

                const promos = await getAllPromotions();
                setPromotions(promos);
            } catch (err) {
                console.error("Error loading data", err);
            }
        };

        fetchData();
    }, []);

    // ============================
    // ADD MAIN IMAGES
    // ============================
    const handleMainImageChange = (e) => {
        const files = Array.from(e.target.files);

        const newFiles = files.map(file => ({
            file,
            preview: URL.createObjectURL(file)
        }));

        setMainImages(prev => [...prev, ...newFiles]);
    };

    const removeMainImage = (index) => {
        setMainImages(prev => {
            URL.revokeObjectURL(prev[index].preview);
            return prev.filter((_, i) => i !== index);
        });
    };

    // ============================
    // SUBMIT FORM
    // ============================
    const handleSubmit = async (e) => {
        e.preventDefault();

        const productData = {
            name,
            description,
            price,
            discountPrice,
            brandId,
            categoryId,
            mainImages,
            details,
            colorVariants
        };

        const validationErrors = validateProduct(productData, { isAdd: true });

        // 3️⃣ Nếu có lỗi → setErrors + ngăn submit
        if (Object.keys(validationErrors).length > 0) {
            setErrors(validationErrors);
            return;
        }

        setErrors({});

        const form = new FormData();

        form.append("name", name);
        form.append("description", description);
        form.append("price", price);
        form.append("discountPrice", discountPrice);
        form.append("brandId", brandId);
        form.append("categoryId", categoryId);
        form.append("isFeatured", isFeatured);

        // Ảnh chính
        mainImages.forEach(m => {
            form.append("imageFiles", m.file);
        });

        // Chi tiết mô tả
        details.forEach((d, i) => {
            form.append(`details[${i}].text`, d.text);
            form.append(`details[${i}].sortOrder`, d.sortOrder);

            if (d.imageFile) {
                form.append(`details[${i}].imageFile`, d.imageFile);
            }
        });

        // Biến thể màu
        colorVariants.forEach((cv, i) => {
            form.append(`colorVariants[${i}].color`, cv.color);

            cv.imageFiles.forEach(fileObj => {
                form.append(`colorVariants[${i}].imageFiles`, fileObj.file);
            });

            cv.sizes.forEach((s, j) => {
                form.append(`colorVariants[${i}].sizes[${j}].size`, s.size);
                form.append(`colorVariants[${i}].sizes[${j}].stock`, s.stock);
            });
        });

        const promotionIds = selectedPromotions.map(p => p.value);
        promotionIds.forEach(id => form.append("promotionIds", id));

        await createProduct(form);
        alert("Tạo sản phẩm thành công!");
        navigate('/admin/product');
    };

    return (
        <div className={styles.container}>
            <h2 className={styles.title}>Thêm sản phẩm</h2>

            <form className={styles.formBox} onSubmit={handleSubmit}>

                {/* ==================== THÔNG TIN CHUNG ==================== */}
                <div className={styles.section}>
                    <h3>Thông tin chung</h3>

                    <input
                        placeholder="Tên sản phẩm"
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                    />
                    {errors.name && <span className={styles.error}>{errors.name}</span>}

                    <textarea
                        placeholder="Mô tả"
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                    />
                    {errors.description && <span className={styles.error}>{errors.description}</span>}

                    <div className={styles.row}>
                        <div>
                            <input
                                type="number"
                                placeholder="Giá"
                                value={price}
                                onChange={(e) => setPrice(e.target.value)}
                            />
                            {errors.price && <span className={styles.error}>{errors.price}</span>}
                        </div>

                        <div><input
                            type="number"
                            placeholder="Giá giảm"
                            value={discountPrice}
                            onChange={(e) => setDiscountPrice(e.target.value)}
                        />
                            {errors.discountPrice && <span className={styles.error}>{errors.discountPrice}</span>}</div>

                    </div>

                    <div className={styles.row}>
                        <div><select
                            value={brandId}
                            onChange={(e) => setBrandId(e.target.value)}
                        >
                            <option value="">-- Chọn thương hiệu --</option>
                            {brands.map((b) => (
                                <option key={b.id} value={b.id}>
                                    {b.name}
                                </option>
                            ))}
                        </select>
                            {errors.brandId && <span className={styles.error}>{errors.brandId}</span>}</div>

                        <div><select
                            value={categoryId}
                            onChange={(e) => setCategoryId(e.target.value)}
                        >
                            <option value="">-- Chọn danh mục --</option>
                            {categories.map((c) => (
                                <option key={c.id} value={c.id}>
                                    {c.name}
                                </option>
                            ))}
                        </select>
                            {errors.categoryId && <span className={styles.error}>{errors.categoryId}</span>}</div>

                    </div>

                    <label className={styles.checkbox}>
                        <input
                            type="checkbox"
                            checked={isFeatured}
                            onChange={(e) => setIsFeatured(e.target.checked)}
                        />
                        <span>Nổi bật</span>
                    </label>
                </div>

                {/* ==================== ẢNH CHÍNH ==================== */}
                <div className={styles.section}>
                    <h3>Ảnh chính</h3>

                    <input type="file" multiple onChange={handleMainImageChange} />
                    {errors.mainImages && <span className={styles.error}>{errors.mainImages}</span>}
                    <div className={styles.previewList}>
                        {mainImages.map((m, i) => (
                            <div key={i} className={styles.previewItem}>
                                <img src={m.preview} alt="preview" />

                                <button
                                    type="button"
                                    className={styles.btnRemoveImage}
                                    onClick={() => removeMainImage(i)}
                                >
                                    ✕
                                </button>
                            </div>
                        ))}
                    </div>
                </div>

                {/* ==================== CHI TIẾT MÔ TẢ ==================== */}
                <div className={styles.section}>
                    <h3>Chi tiết mô tả</h3>

                    {details.map((d, idx) => (
                        <div key={idx} className={styles.variantBox}>
                            <textarea
                                placeholder="Nội dung mô tả"
                                value={d.text}
                                onChange={(e) => {
                                    const copy = [...details];
                                    copy[idx].text = e.target.value;
                                    setDetails(copy);
                                }}
                            />
                            {errors[`details[${idx}]`] && <span className={styles.error}>{errors[`details[${idx}]`]}</span>}

                            <input
                                type="number"
                                placeholder="Sort Order"
                                value={d.sortOrder}
                                onChange={(e) => {
                                    const copy = [...details];
                                    copy[idx].sortOrder = e.target.value;
                                    setDetails(copy);
                                }}
                            />

                            <input
                                type="file"
                                onChange={(e) => {
                                    const file = e.target.files[0];
                                    const copy = [...details];
                                    copy[idx].imageFile = file;
                                    copy[idx].preview = URL.createObjectURL(file);
                                    setDetails(copy);
                                }}
                            />

                            {d.preview && (
                                <div className={styles.previewItem}>
                                    <img src={d.preview} alt="" />
                                    <button
                                        type="button"
                                        className={styles.btnRemoveImage}
                                        onClick={() => {
                                            const copy = [...details];
                                            URL.revokeObjectURL(copy[idx].preview);
                                            copy[idx].imageFile = null;
                                            copy[idx].preview = null;
                                            setDetails(copy);
                                        }}
                                    >
                                        ✕
                                    </button>
                                </div>
                            )}

                            <button
                                type="button"
                                className={styles.btnRemove}
                                onClick={() =>
                                    setDetails(details.filter((_, i) => i !== idx))
                                }
                            >
                                Xóa mô tả
                            </button>
                        </div>
                    ))}

                    <button
                        type="button"
                        className={styles.btnAdd}
                        onClick={() =>
                            setDetails([
                                ...details,
                                { text: "", sortOrder: 0, imageFile: null, preview: null }
                            ])
                        }
                    >
                        + Thêm mô tả
                    </button>
                </div>

                {/* ==================== BIẾN THỂ MÀU ==================== */}
                <div className={styles.section}>
                    <h3>Biến thể màu</h3>

                    {colorVariants.map((cv, i) => (
                        <div key={i} className={styles.variantBox}>
                            <input
                                placeholder="Tên màu"
                                value={cv.color}
                                onChange={(e) => {
                                    const copy = [...colorVariants];
                                    copy[i].color = e.target.value;
                                    setColorVariants(copy);
                                }}
                            />
                            {errors[`colorVariants[${i}]`] && <span className={styles.error}>{errors[`colorVariants[${i}]`]}</span>}

                            {/* UPLOAD ẢNH MÀU */}
                            <input
                                type="file"
                                multiple
                                onChange={(e) => {
                                    const files = Array.from(e.target.files).map(f => ({
                                        file: f,
                                        preview: URL.createObjectURL(f)
                                    }));

                                    const copy = [...colorVariants];
                                    copy[i].imageFiles.push(...files);
                                    setColorVariants(copy);
                                }}
                            />

                            {/* PREVIEW ẢNH */}
                            <div className={styles.previewList}>
                                {cv.imageFiles.map((img, j) => (
                                    <div key={j} className={styles.previewItem}>
                                        <img src={img.preview} alt="" />

                                        <button
                                            type="button"
                                            className={styles.btnRemoveImage}
                                            onClick={() => {
                                                const copy = [...colorVariants];
                                                URL.revokeObjectURL(img.preview);
                                                copy[i].imageFiles = copy[i].imageFiles.filter(
                                                    (_, k) => k !== j
                                                );
                                                setColorVariants(copy);
                                            }}
                                        >
                                            ✕
                                        </button>
                                    </div>
                                ))}
                            </div>

                            {/* SIZES */}
                            <h4>Sizes</h4>

                            {cv.sizes.map((s, j) => (
                                <div key={j} className={styles.sizeRow}>
                                    <div>
                                        <input
                                            placeholder="Size"
                                            value={s.size}
                                            onChange={(e) => {
                                                const copy = [...colorVariants];
                                                copy[i].sizes[j].size = e.target.value;
                                                setColorVariants(copy);
                                            }}
                                        />
                                        {errors[`colorVariants[${i}].sizes[${j}].size`] && <span className={styles.error}>{errors[`colorVariants[${i}].sizes[${j}].size`]}</span>}

                                    </div>
                                    <div><input
                                        type="number"
                                        placeholder="Tồn kho"
                                        value={s.stock}
                                        onChange={(e) => {
                                            const copy = [...colorVariants];
                                            copy[i].sizes[j].stock = e.target.value;
                                            setColorVariants(copy);
                                        }}
                                    />
                                        {errors[`colorVariants[${i}].sizes[${j}].stock`] && <span className={styles.error}>{errors[`colorVariants[${i}].sizes[${j}].stock`]}</span>}</div>


                                    <button
                                        type="button"
                                        className={styles.btnRemove}
                                        onClick={() => {
                                            const copy = [...colorVariants];
                                            copy[i].sizes = copy[i].sizes.filter(
                                                (_, k) => k !== j
                                            );
                                            setColorVariants(copy);
                                        }}
                                    >
                                        Xóa
                                    </button>
                                </div>
                            ))}

                            {/* ACTION BUTTONS */}
                            <div className={styles.variantActions}>
                                <button
                                    type="button"
                                    className={styles.btnAdd}
                                    onClick={() => {
                                        const copy = [...colorVariants];
                                        copy[i].sizes.push({ size: "", stock: 0 });
                                        setColorVariants(copy);
                                    }}
                                >
                                    + Thêm size
                                </button>

                                <button
                                    type="button"
                                    className={styles.btnRemove}
                                    onClick={() =>
                                        setColorVariants(
                                            colorVariants.filter((_, idx) => idx !== i)
                                        )
                                    }
                                >
                                    Xóa màu
                                </button>
                            </div>
                        </div>
                    ))}

                    <button
                        type="button"
                        className={styles.btnAdd}
                        onClick={() =>
                            setColorVariants([
                                ...colorVariants,
                                {
                                    color: "",
                                    imageFiles: [],
                                    sizes: [{ size: "", stock: 0 }]
                                }
                            ])
                        }
                    >
                        + Thêm màu
                    </button>
                </div>

                <div className={styles.section}>
                    <h3>Ưu đãi</h3>
                    <Select
                        options={promotions.map(p => ({ value: p.id, label: p.name }))}
                        isMulti
                        value={selectedPromotions}
                        onChange={setSelectedPromotions}
                        placeholder="Chọn ưu đãi..."
                        classNamePrefix="select"
                        noOptionsMessage={() => "Không có ưu đãi"}
                    />
                </div>

                <button type="submit" className={styles.btnSubmit}>
                    Lưu sản phẩm
                </button>
            </form>
        </div>
    );
};

export default AddProductPage;