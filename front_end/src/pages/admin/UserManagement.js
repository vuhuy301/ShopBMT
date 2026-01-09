import React, { useEffect, useState } from "react";
import styles from "./UserManagement.module.css";
import { getUsers, createEmployee, toggleUserActive } from "../../services/admin/userService";
import { getRoles } from "../../services/roleService";

export default function UserManagement() {
    // === STATE ===
    const [users, setUsers] = useState([]);
    const [roles, setRoles] = useState([]);

    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize] = useState(6);

    const [roleId, setRoleId] = useState("");
    const [emailSearch, setEmailSearch] = useState("");

    const [totalPages, setTotalPages] = useState(1);
    const [loading, setLoading] = useState(false);

    // === Modal create user ===
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [newFullName, setNewFullName] = useState("");
    const [newEmail, setNewEmail] = useState("");
    const [newPassword, setNewPassword] = useState("");
    const [newRoleId, setNewRoleId] = useState("");
    const [creating, setCreating] = useState(false);

    const [createError, setCreateError] = useState("");
    const [formErrors, setFormErrors] = useState({});


    // === LOAD ROLES ===
    useEffect(() => {
        const loadRoles = async () => {
            try {
                const data = await getRoles();
                setRoles(data);
            } catch (error) {
                console.error("Load roles error:", error);
            }
        };
        loadRoles();
    }, []);

    // === LOAD USERS ===
    const loadData = async () => {
        try {
            setLoading(true);
            const result = await getUsers({
                roleId: roleId || null,
                pageNumber,
                pageSize,
                email: emailSearch || null,
            });
            setUsers(result.items);
            setTotalPages(result.totalPages);
        } catch (error) {
            console.error("Load users error:", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData();
    }, [pageNumber, roleId, emailSearch]);

    const validateCreateUser = () => {
        const errors = {};

        // === FULL NAME ===
        if (!newFullName.trim()) {
            errors.fullName = "Họ tên không được để trống";
        } else if (newFullName.trim().length < 3) {
            errors.fullName = "Họ tên phải ít nhất 3 ký tự";
        }
        if (!newEmail.trim()) {
            errors.email = "Email không được để trống";
        } else {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(newEmail)) {
                errors.email = "Email không hợp lệ";
            }
        }

        // === PASSWORD (≥ 6 ký tự) ===
        if (!newPassword) {
            errors.password = "Mật khẩu không được để trống";
        } else if (newPassword.length < 6) {
            errors.password = "Mật khẩu phải có ít nhất 6 ký tự";
        }

        setFormErrors(errors);
        return Object.keys(errors).length === 0;
    };



    // === HANDLE CREATE USER ===
    const handleCreateUser = async (e) => {
        e.preventDefault();

        if (!validateCreateUser()) return;

        setCreating(true);
        setCreateError(""); // reset lỗi cũ

        try {
            await createEmployee({
                fullName: newFullName,
                email: newEmail,
                password: newPassword,
                role: newRoleId,
            });

            setIsModalOpen(false);
            setNewFullName("");
            setNewEmail("");
            setNewPassword("");
            setNewRoleId("");
            loadData();
        } catch (error) {
            setCreateError(error.message); // 👈 HIỂN THỊ LỖI
        } finally {
            setCreating(false);
        }
    };


    return (
        <div className={styles.container}>
            <div className={styles.title}>Quản lý người dùng</div>

            {/* --- Nút tạo nhân viên --- */}
            <button
                className='btn btn-success mb-2'
                onClick={() => setIsModalOpen(true)}
            >
                + Tạo nhân viên
            </button>

            {/* SEARCH + FILTER */}
            <div className={styles.form}>
                <select
                    className={styles.roleSelect}
                    value={roleId}
                    onChange={(e) => {
                        setRoleId(e.target.value);
                        setPageNumber(1);
                    }}
                >
                    <option value="">Tất cả role</option>
                    {roles.map((r) => (
                        <option key={r.id} value={r.id}>
                            {r.name}
                        </option>
                    ))}
                </select>

                <input
                    className={styles.searchInput}
                    placeholder="Tìm email..."
                    value={emailSearch}
                    onChange={(e) => setEmailSearch(e.target.value)}
                />
            </div>

            {/* TABLE */}
            {loading ? (
                <p>Đang tải...</p>
            ) : (
                <table className={styles.table}>
                    <thead>
                        <tr>
                            <th>Họ tên</th>
                            <th>Email</th>
                            <th>Role</th>
                            <th>Kích hoạt</th>
                            <th>Ngày tạo</th>
                            <th>Hành động</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users?.length ? (
                            users.map((u) => (
                                <tr key={u.id}>
                                    <td>{u.fullName}</td>
                                    <td>{u.email}</td>
                                    <td>{u.roleName}</td>
                                    <td className={u.isActive ? styles.statusActive : styles.statusInactive}>
                                        {u.isActive ? "Hoạt động" : "Khoá"}
                                    </td>
                                    <td>{new Date(u.createdAt).toLocaleDateString("vi-VN")}</td>
                                    <td>
                                        {u.roleName === "Admin" ? (
                                            <button className={styles.disabledBtn} disabled>
                                                Không thể khóa
                                            </button>
                                        ) : (
                                            <button
                                                className={u.isActive ? styles.deleteBtn : styles.editBtn}
                                                onClick={async () => {
                                                    const action = u.isActive ? "khóa" : "mở";
                                                    const confirm = window.confirm(`Bạn có chắc muốn ${action} người dùng này không?`);
                                                    if (!confirm) return;

                                                    try {
                                                        await toggleUserActive(u.id, !u.isActive);
                                                        loadData(); // load lại danh sách
                                                        alert(`Người dùng đã được ${action} thành công.`);
                                                    } catch (err) {
                                                        console.error(err);
                                                        alert("Thao tác thất bại");
                                                    }
                                                }}
                                            >
                                                {u.isActive ? "Khoá" : "Mở"}
                                            </button>
                                        )}
                                    </td>

                                </tr>
                            ))
                        ) : (
                            <tr>
                                <td colSpan="6" style={{ textAlign: "center" }}>
                                    Không có dữ liệu
                                </td>
                            </tr>
                        )}
                    </tbody>
                </table>
            )}

            {/* PAGINATION */}
            <div className={styles.pagination}>
                <button
                    disabled={pageNumber === 1}
                    onClick={() => setPageNumber(pageNumber - 1)}
                >
                    ← Trước
                </button>

                <span>
                    Trang {pageNumber} / {totalPages}
                </span>

                <button
                    disabled={pageNumber === totalPages}
                    onClick={() => setPageNumber(pageNumber + 1)}
                >
                    Sau →
                </button>
            </div>

            {/* --- Modal tạo user --- */}
            {isModalOpen && (
                <div className={styles.modalOverlay}>
                    <div className={styles.modalContent}>
                        <h3>Tạo tài khoản nhân viên</h3>
                        {createError && (
                            <div className={styles.errorMessage}>
                                {createError}
                            </div>
                        )}

                        <form onSubmit={handleCreateUser} className={styles.modalForm}>
                            <input
                                type="text"
                                placeholder="Họ tên"
                                value={newFullName}
                                onChange={(e) => setNewFullName(e.target.value)}
                            />
                            {formErrors.fullName && (
                                <div className={styles.fieldError}>{formErrors.fullName}</div>
                            )}
                            <input
                                type="email"
                                placeholder="Email"
                                value={newEmail}
                                onChange={(e) => setNewEmail(e.target.value)}
                            />
                            {formErrors.email && (
                                <div className={styles.fieldError}>{formErrors.email}</div>
                            )}
                            <input
                                type="password"
                                placeholder="Mật khẩu"
                                value={newPassword}
                                onChange={(e) => setNewPassword(e.target.value)}
                            />
                            {formErrors.password && (
                                <div className={styles.fieldError}>{formErrors.password}</div>
                            )}

                            <select
                                value={newRoleId}
                                onChange={(e) => setNewRoleId(e.target.value)}
                                required
                            >
                                <option value="">Chọn role</option>
                                {roles.map((r) => (
                                    <option key={r.id} value={r.name}>
                                        {r.name}
                                    </option>
                                ))}
                            </select>


                            <div className={styles.modalButtons}>
                                <button type="submit" disabled={creating}>
                                    {creating ? "Đang tạo..." : "Tạo"}
                                </button>
                                <button type="button" onClick={() => setIsModalOpen(false)}>
                                    Huỷ
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}
