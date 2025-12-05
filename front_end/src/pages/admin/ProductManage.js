import React from "react";
import "./ProducManage.module.css"
const ProducManage = ({ products, onEdit, onDelete }) => {
  if (products.length === 0) {
    return <div className="text-center py-5 text-muted">Chưa có sản phẩm nào</div>;
  }

  return (
    <div className="table-responsive">
      <table className="table table-hover">
        <thead className="table-light">
          <tr>
            <th>Ảnh</th>
            <th>Tên</th>
            <th>Giá</th>a
            <th>Giá giảm</th>
            <th>Tồn kho</th>
            <th>Hành động</th>
          </tr>
        </thead>
        <tbody>
          {products.map((p) => (
            <tr key={p.id}>
              <td>
                <img
                  src={p.images?.[0]?.imageUrl || "https://via.placeholder.com/60"}
                  alt=""
                  width="60"
                  style={{ borderRadius: 6 }}
                />
              </td>
              <td>{p.name}</td>
              <td>{p.price?.toLocaleString()}đ</td>
              <td>{p.discountPrice ? <strong className="text-danger">{p.discountPrice.toLocaleString()}đ</strong> : "-"}</td>
              <td>{p.stock || 0}</td>
              <td>
                <button className="btn btn-sm btn-warning me-2" onClick={() => onEdit && onEdit(p)}>
                  Add
                </button>
                <button className="btn btn-sm btn-warning me-2" onClick={() => onEdit && onEdit(p)}>
                  Sửa
                </button>
                <button className="btn btn-sm btn-danger" onClick={() => onDelete(p.id)}>
                  Xóa
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default ProducManage;