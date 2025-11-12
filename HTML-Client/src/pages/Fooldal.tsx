import { useEffect, useState } from "react";
import type { Szerzo } from "../types/Szerzo";
import type { Cikk } from "../types/Cikk";
import { toast } from "react-toastify";
import apiClient from "../api/apiClient";

const Fooldal = () => {
  const [szerzok, setSzerzok] = useState<Array<Szerzo>>([]);

  useEffect(() => {
    apiClient
      .get("/get")
      .then((Response) => setSzerzok(Response.data))
      .catch(() => toast.error("A szerzők belöltése sikertelen!"));
  }, []);

  return (
    <>
      <h1>Cikkek</h1>
      {szerzok.map((s) => (
        <div className="card">
            <h2>{s.nev}</h2>
            <p>{s.id}</p>
            {s.cikkek.map((c) => (
                <div>
                    <h3>{c.cim}</h3>
                    <p>{c.id}</p>
                    <p>{c.leiras}</p>
                </div>
            ))}
        </div>
      ))}
    </>
  );
};
export default Fooldal;
