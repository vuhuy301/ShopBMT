// src/components/Chatbot.js
import React, { useState, useEffect, useRef } from "react";
import styles from "./ChatBot.module.css";

const Chatbot = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState([]);
  const [input, setInput] = useState("");
  const [isTyping, setIsTyping] = useState(false);
  const messagesEndRef = useRef(null);

  const API_URL = "http://localhost:8000";
  const JWT_KEY = "accessToken"; // ← Đúng với LoginPage.js của bạn

  // Lấy token + giải mã lấy user_id
  const getToken = () => localStorage.getItem(JWT_KEY);
  const getUserId = () => {
    const token = getToken();
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split(".")[1]));
      return payload.sub; // ← "2", "15", v.v.
    } catch {
      return null;
    }
  };

  // Session cho khách vãng lai (hết hạn sau 7 ngày)
  const getGuestSessionId = () => {
    const sid = localStorage.getItem("chat_session_id");
    const expires = localStorage.getItem("chat_session_expires");
    if (sid && expires && new Date(expires) > new Date()) {
      return sid;
    }
    localStorage.removeItem("chat_session_id");
    localStorage.removeItem("chat_session_expires");
    return null;
  };

  const saveGuestSessionId = (sid) => {
    localStorage.setItem("chat_session_id", sid);
    const expires = new Date();
    expires.setDate(expires.getDate() + 7); // 7 ngày
    localStorage.setItem("chat_session_expires", expires.toISOString());
  };

  // Load lịch sử khi mở chatbot
  useEffect(() => {
    if (!isOpen) return;

    const userId = getUserId();
    if (userId) {
      // Đã đăng nhập → lấy lịch sử theo user_id
      fetch(`${API_URL}/my_chat_history?user_id=${userId}`)
        .then(r => r.json())
        .then(data => {
          if (data.messages?.length > 0) {
            setMessages(data.messages.map(m => ({
              role: m.role === "user" ? "user" : "ai",
              content: m.content
            })));
          } else {
            setMessages([{ role: "ai", content: "Dạ em chào anh/chị! Hôm nay cần tư vấn vợt nào ạ?" }]);
          }
        })
        .catch(() => setMessages([{ role: "ai", content: "Dạ chào anh/chị! Linh sẵn sàng hỗ trợ rồi ạ" }]));
    } else {
      // Chưa đăng nhập → dùng session_id tạm
      setMessages([{ role: "ai", content: "Dạ em chào anh/chị! Linh đây ạ, anh/chị cần tư vấn gì nè?" }]);
    }
  }, [isOpen]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const sendMessage = async () => {
    if (!input.trim() || isTyping) return;

    const userMsg = { role: "user", content: input };
    setMessages(prev => [...prev, userMsg]);
    setInput("");
    setIsTyping(true);

    const userId = getUserId();
    const guestSessionId = getGuestSessionId();

    try {
      const res = await fetch(`${API_URL}/chat`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          question: input,
          user_id: userId || undefined,
          session_id: !userId ? guestSessionId : undefined
        }),
      });

      const data = await res.json();

      if (data.session_id && !userId) {
        saveGuestSessionId(data.session_id);
      }

      setMessages(prev => [...prev, { role: "ai", content: data.answer }]);
    } catch (err) {
      setMessages(prev => [...prev, { role: "ai", content: "Oops! Linh đang hơi chậm, anh/chị thử lại nha~" }]);
    } finally {
      setIsTyping(false);
    }
  };

  return (
    <>
      <button onClick={() => setIsOpen(!isOpen)} className={styles.floatingButton}>
        <span className={styles.icon}>{isOpen ? "×" : "Chat"}</span>
        {!isOpen && <span className={styles.pulse}></span>}
      </button>

      {isOpen && (
        <div className={styles.chatWidget}>
          <div className={styles.header}>
            <div className={styles.avatar}>
              <img src="https://i.ibb.co.com/0jZJ7YJ/linh-cute.png" alt="Linh" className={styles.avatarImg} />
            </div>
            <div className={styles.headerInfo}>
              <h3>Linh – Tư vấn cầu lông 24/7</h3>
              <p className={styles.online}>● Đang online</p>
            </div>
            <button onClick={() => setIsOpen(false)} className={styles.closeBtn}>×</button>
          </div>

          <div className={styles.messagesContainer}>
            {messages.map((msg, i) => (
              <div key={i} className={`${styles.message} ${msg.role === "user" ? styles.userMessage : styles.botMessage}`}>
                {msg.role === "ai" && (
                  <img src="https://i.ibb.co.com/0jZJ7YJ/linh-cute.png" alt="Linh" className={styles.msgAvatar} />
                )}
                <div className={styles.bubble}>{msg.content}</div>
              </div>
            ))}
            {isTyping && (
              <div className={styles.message}>
                <img src="https://i.ibb.co.com/0jZJ7YJ/linh-cute.png" alt="Linh" className={styles.msgAvatar} />
                <div className={styles.typingBubble}><span></span><span></span><span></span></div>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>

          <div className={styles.inputContainer}>
            <input
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onKeyPress={(e) => e.key === "Enter" && !e.shiftKey && sendMessage()}
              placeholder="Nhập tin nhắn cho Linh..."
              className={styles.input}
            />
            <button onClick={sendMessage} className={styles.sendButton}>Gửi</button>
          </div>
        </div>
      )}
    </>
  );
};

export default Chatbot;
