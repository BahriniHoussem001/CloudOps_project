import { useEffect, useState } from "react";
import apiClient from "../api/apiClient";

function Users() {
  const [users, setUsers] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    apiClient
      .get("/users")
      .then((response) => {
        setUsers(response.data.data);
      })
      .catch(() => {
        setError("Failed to load users");
      });
  }, []);

  return (
    <div>
      <h1>Users</h1>
      <p>Application users loaded from the backend.</p>

      {error && <p className="error">{error}</p>}

      <div className="card">
        {users.length === 0 ? (
          <p>No users found.</p>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Full name</th>
                <th>Email</th>
                <th>Role</th>
              </tr>
            </thead>

            <tbody>
              {users.map((user) => (
                <tr key={user.id}>
                  <td>{user.fullName}</td>
                  <td>{user.email}</td>
                  <td>{user.role}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}

export default Users;