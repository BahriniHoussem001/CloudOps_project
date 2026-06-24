import { useEffect, useState } from "react";
import apiClient from "../api/apiClient";

function Dashboard() {
  const [health, setHealth] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    apiClient
      .get("/health")
      .then((response) => {
        setHealth(response.data.data);
      })
      .catch(() => {
        setError("Backend is not reachable");
      });
  }, []);

  return (
    <div>
      <h1>CloudOps Dashboard</h1>
      <p>Backend health status</p>

      {error && <p className="error">{error}</p>}

      {health && (
        <div className="card">
          <p>
            <strong>API:</strong> {health.api}
          </p>
          <p>
            <strong>Database:</strong> {health.database}
          </p>
          <p>
            <strong>RabbitMQ:</strong> {health.rabbitMq}
          </p>
          <p>
            <strong>Status:</strong> {health.status}
          </p>
        </div>
      )}
    </div>
  );
}

export default Dashboard;