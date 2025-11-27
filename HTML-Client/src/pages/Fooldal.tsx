import { useEffect, useState } from "react";
import type { Szerzo } from "../types/Szerzo";
import { toast } from "react-toastify";
import apiClient from "../api/apiClient";
import type { Cikk } from "../types/Cikk";
import { spread } from "axios";

const Fooldal = () => {
  const [szerzok, setSzerzok] = useState<Array<Szerzo>>([]);
  const [cikkek, setCikkek] = useState<Array<Cikk>>([]);

  useEffect(() => {
    apiClient
      .get("/authors/listauthors")
      .then((response) => setSzerzok(response.data))
      .catch(() => toast.error("A szerzők belöltése sikertelen!"));
  }, []);

  useEffect(() => {
    apiClient
      .get("/articles")
      .then((response) => setCikkek(response.data))
      .catch(() => toast.error("A cikkek belöltése sikertelen!"));
  }, []);

  return (
    <>
      <h1>Cikkek</h1>
      {szerzok.map((s) => (
        <div className="card">
          <span className="cim">{s.name}</span> --- <span>{s.id}</span>
          <hr />
          {cikkek.map((c) => (
            <div className="cikk_cim">
              {c.authorId == s.id && (
                <div>
                  <span>
                    {c.title} - {c.id}
                  </span>
                  <p className="cikk_leiras">{c.description}</p>
                  <hr />
                </div>
              )}
              
            </div>
          ))}
        </div>
      ))}
    </>
  );
};
export default Fooldal;
