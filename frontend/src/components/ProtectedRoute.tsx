/* eslint-disable react-hooks/rules-of-hooks */
import {useContext, ReactNode} from "react";
import { Navigate } from "react-router-dom";
import { AuthContext } from "../context/AuthProvider";

interface ProtectedRouteProps {
  children: ReactNode;
}


const ProtectedRoute = ({children}: ProtectedRouteProps) =>{
    const {user} = useContext(AuthContext)!
    return user ? <>{children}</> : <Navigate to="/login" />
}

export default ProtectedRoute;