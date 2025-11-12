import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import Fooldal from './pages/Fooldal'
import { ToastContainer } from 'react-toastify'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Fooldal />
    <ToastContainer theme='colored' position='top-center'/>
  </StrictMode>,
)
