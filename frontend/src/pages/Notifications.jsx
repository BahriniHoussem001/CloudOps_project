import { useEffect, useState } from "react";
import apiClient from "../api/apiClient";

function Notifications() {
  const [users, setUsers] = useState([]);
  const [selectedUserId, setSelectedUserId] = useState("");
  const [notifications, setNotifications] = useState([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [error, setError] = useState("");

  const loadUsers = async () => {
    try {
      const response = await apiClient.get("/users");
      const loadedUsers = response.data.data;

      setUsers(loadedUsers);

      if (loadedUsers.length > 0) {
        setSelectedUserId(loadedUsers[0].id);
      }
    } catch {
      setError("Failed to load users");
    }
  };

  const loadNotifications = async (userId) => {
    if (!userId) return;

    try {
      const [notificationsResponse, unreadCountResponse] = await Promise.all([
        apiClient.get(`/users/${userId}/notifications`),
        apiClient.get(`/users/${userId}/notifications/unread-count`),
      ]);

      setNotifications(notificationsResponse.data.data);
      setUnreadCount(unreadCountResponse.data.data.unreadCount);
    } catch {
      setError("Failed to load notifications");
    }
  };

  useEffect(() => {
    loadUsers();
  }, []);

  useEffect(() => {
    loadNotifications(selectedUserId);
  }, [selectedUserId]);

  const markAsRead = async (notificationId) => {
    try {
      await apiClient.put(`/notifications/${notificationId}/mark-as-read`);
      await loadNotifications(selectedUserId);
    } catch {
      setError("Failed to mark notification as read");
    }
  };

  const markAllAsRead = async () => {
    try {
      await apiClient.put(
        `/users/${selectedUserId}/notifications/mark-all-as-read`
      );
      await loadNotifications(selectedUserId);
    } catch {
      setError("Failed to mark all notifications as read");
    }
  };

  return (
    <div>
      <h1>Notifications</h1>
      <p>User notifications loaded from the backend.</p>

      {error && <p className="error">{error}</p>}

      <div className="card">
        <label>Select user</label>
        <select
          value={selectedUserId}
          onChange={(event) => setSelectedUserId(event.target.value)}
        >
          {users.map((user) => (
            <option key={user.id} value={user.id}>
              {user.fullName}
            </option>
          ))}
        </select>

        <p>
          <strong>Unread notifications:</strong> {unreadCount}
        </p>

        <button className="primary-button" onClick={markAllAsRead}>
          Mark all as read
        </button>
      </div>

      <div className="card">
        <h2>Notifications list</h2>

        {notifications.length === 0 ? (
          <p>No notifications found.</p>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Title</th>
                <th>Message</th>
                <th>Status</th>
                <th>Action</th>
              </tr>
            </thead>

            <tbody>
              {notifications.map((notification) => (
                <tr key={notification.id}>
                  <td>{notification.title}</td>
                  <td>{notification.message}</td>
                  <td>{notification.isRead ? "Read" : "Unread"}</td>
                  <td>
                    {!notification.isRead && (
                      <button
                        className="small-button"
                        onClick={() => markAsRead(notification.id)}
                      >
                        Mark as read
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}

export default Notifications;