import { initializeApp } from 'https://www.gstatic.com/firebasejs/11.0.2/firebase-app.js';
import {
    getAuth,
    onAuthStateChanged,
    signInWithPopup,
    signInWithEmailAndPassword,
    createUserWithEmailAndPassword,
    sendPasswordResetEmail,
    updateProfile,
    signOut,
    GoogleAuthProvider,
    GithubAuthProvider,
    OAuthProvider
} from 'https://www.gstatic.com/firebasejs/11.0.2/firebase-auth.js';

let auth = null;
let dotNetRef = null;

// Resolves once Firebase has restored (or ruled out) a persisted session. Every call
// that needs the current user awaits this, so nothing reads a null user during the
// gap between page load and Firebase rehydrating from IndexedDB.
let readyPromise = null;

export function initialize(config, dotNetHelper) {
    if (auth) return;

    const app = initializeApp(config);
    auth = getAuth(app);
    dotNetRef = dotNetHelper;

    readyPromise = new Promise(resolve => {
        let settled = false;
        onAuthStateChanged(auth, user => {
            if (!settled) {
                settled = true;
                resolve();
                return;
            }
            // Only changes AFTER the initial restore are worth waking Blazor for;
            // the first callback is just the session being rehydrated.
            dotNetRef?.invokeMethodAsync('OnAuthStateChanged', !!user);
        });
    });
}

async function currentUser() {
    await readyPromise;
    return auth.currentUser;
}

function providerFor(providerId) {
    switch (providerId) {
        case 'google':    return new GoogleAuthProvider();
        case 'github':    return new GithubAuthProvider();
        case 'microsoft': return new OAuthProvider('microsoft.com');
        default: throw new Error(`Unknown provider: ${providerId}`);
    }
}

export async function isAuthenticated() {
    return !!(await currentUser());
}

// forceRefresh is driven by the caller: it asks for a fresh token only once the one
// it holds has actually expired, rather than on every request.
export async function getAuth_(forceRefresh) {
    const user = await currentUser();
    if (!user) return null;

    const result = await user.getIdTokenResult(forceRefresh === true);

    return {
        token: result.token,
        uid: user.uid,
        email: user.email,
        displayName: user.displayName,
        photoUrl: user.photoURL,
        expiresAt: result.expirationTime
    };
}

export async function signInWithProvider(providerId) {
    try {
        await signInWithPopup(auth, providerFor(providerId));
        return null;
    } catch (error) {
        return toMessage(error);
    }
}

export async function signInWithEmail(email, password) {
    try {
        await signInWithEmailAndPassword(auth, email, password);
        return null;
    } catch (error) {
        return toMessage(error);
    }
}

export async function registerWithEmail(email, password, displayName) {
    try {
        const credential = await createUserWithEmailAndPassword(auth, email, password);
        if (displayName) await updateProfile(credential.user, { displayName });
        return null;
    } catch (error) {
        return toMessage(error);
    }
}

export async function sendPasswordReset(email) {
    try {
        await sendPasswordResetEmail(auth, email);
        return null;
    } catch (error) {
        return toMessage(error);
    }
}

export async function signOutUser() {
    await signOut(auth);
}

function toMessage(error) {
    switch (error?.code) {
        case 'auth/invalid-email':            return 'That email address is not valid.';
        case 'auth/user-disabled':            return 'This account has been disabled.';
        case 'auth/user-not-found':
        case 'auth/wrong-password':
        case 'auth/invalid-credential':       return 'Incorrect email or password.';
        case 'auth/email-already-in-use':     return 'An account already exists for that email.';
        case 'auth/weak-password':            return 'Choose a password with at least 6 characters.';
        case 'auth/popup-closed-by-user':
        case 'auth/cancelled-popup-request':  return 'Sign-in was cancelled.';
        case 'auth/popup-blocked':            return 'Your browser blocked the sign-in popup. Allow popups and try again.';
        case 'auth/account-exists-with-different-credential':
            return 'You already have an account with this email using a different sign-in method.';
        case 'auth/too-many-requests':        return 'Too many attempts. Please try again in a moment.';
        default:                              return error?.message ?? 'Sign-in failed. Please try again.';
    }
}
