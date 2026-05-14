import { initializeApp } from "https://www.gstatic.com/firebasejs/10.11.1/firebase-app.js";
import { getFirestore, collection, addDoc, serverTimestamp } from "https://www.gstatic.com/firebasejs/10.11.1/firebase-firestore.js";

const firebaseConfig = {
  apiKey: "AIzaSyBDFlIhLL-l8D1GQP_RYiRkp0gfJs-EbUc",
  authDomain: "gymio-raj123.firebaseapp.com",
  projectId: "gymio-raj123",
  storageBucket: "gymio-raj123.appspot.com",
  messagingSenderId: "TU_MESSAGING_SENDER_ID",
  appId: "TU_APP_ID"
};

let app, db;
let firebaseInitialized = false;

try {
    app = initializeApp(firebaseConfig);
    db = getFirestore(app);
    firebaseInitialized = true;
} catch (error) {
    // Silencio para no alertar al usuario final
}

document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('prospectoForm');
    const submitBtn = document.getElementById('submitBtn');
    const spinner = document.getElementById('spinner');
    const formMessage = document.getElementById('formMessage');
    const formMessageText = document.getElementById('formMessageText');

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        submitBtn.disabled = true;
        spinner.classList.remove('hidden');
        formMessage.classList.add('hidden');
        formMessage.className = 'hidden rounded-2xl p-4 text-sm font-medium transition-all duration-300 flex items-center gap-3'; 

        const nombre = document.getElementById('nombre').value.trim();
        const email = document.getElementById('email').value.trim();
        const gimnasio = document.getElementById('gimnasio').value.trim();
        const telefono = document.getElementById('telefono').value.trim();

        if (!nombre || !email || !gimnasio || !telefono) {
            formMessageText.textContent = "Por favor, completa todos los campos.";
            formMessage.classList.add('bg-red-500/10', 'text-red-400', 'border', 'border-red-500/20');
            formMessage.classList.remove('hidden');
            submitBtn.disabled = false;
            spinner.classList.add('hidden');
            return;
        }

        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email)) {
            formMessageText.textContent = "Por favor, ingresa un correo electrónico válido.";
            formMessage.classList.add('bg-red-500/10', 'text-red-400', 'border', 'border-red-500/20');
            formMessage.classList.remove('hidden');
            submitBtn.disabled = false;
            spinner.classList.add('hidden');
            return;
        }

        const telefonoRegex = /^\+?[\d\s-]{8,20}$/;
        if (!telefonoRegex.test(telefono)) {
            formMessageText.textContent = "Por favor, ingresa un número de teléfono válido (mín. 8 dígitos).";
            formMessage.classList.add('bg-red-500/10', 'text-red-400', 'border', 'border-red-500/20');
            formMessage.classList.remove('hidden');
            submitBtn.disabled = false;
            spinner.classList.add('hidden');
            return;
        }

        try {
            if (!firebaseInitialized) {
                throw new Error("Generic error");
            }

            const docRef = await addDoc(collection(db, "Prospectos"), {
                nombre: nombre,
                email: email,
                gimnasio: gimnasio,
                telefono: telefono,
                fecha: serverTimestamp(),
                estado: "Nuevo"
            });

            formMessageText.textContent = "¡Gracias! Hemos recibido tus datos correctamente. Pronto te contactaremos.";
            formMessage.classList.add('bg-emerald-500/10', 'text-emerald-400', 'border', 'border-emerald-500/20');
            formMessage.classList.remove('hidden');
            form.reset();

        } catch (error) {
            formMessageText.textContent = "Hubo un error al enviar el formulario. Inténtalo de nuevo más tarde.";
            formMessage.classList.add('bg-red-500/10', 'text-red-400', 'border', 'border-red-500/20');
            formMessage.classList.remove('hidden');
        } finally {
            submitBtn.disabled = false;
            spinner.classList.add('hidden');
        }
    });
});
