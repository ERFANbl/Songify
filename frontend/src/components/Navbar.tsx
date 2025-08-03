/* eslint-disable @typescript-eslint/no-unused-vars */
import { useState, useContext } from "react";
import { Link } from "react-router-dom";
import { AuthContext } from "../context/AuthProvider";
import {logout} from '../services/api'
import "../App.css";
import Logo from "../assets/songify.png";
import { Input } from "@headlessui/react";
import { FaHome, FaSearch, FaTimes } from "react-icons/fa";

const Navbar = () => {
  const [isFocused, setIsFocused] = useState(false);
  const [value, setValue] = useState("");
  const {user, logout: authLogout} = useContext(AuthContext)!

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    console.log("Form submitted");
  };

  const handleClearInput = () => {
    setValue("");
  };

  const handleLogout = async () =>{
    try{
      await logout()
      authLogout()
    } catch(err){
      console.error('Logout Failed', err)
    }
  }

  return (
    <nav className="relative w-full flex items-center justify-between bg-primary px-4 py-8">
      <Link to="/">
        <img src={Logo} className="w-14 h-14" />
      </Link>
      <div className="flex justify-center items-center gap-4">
        <Link to="/" className="cursor-pointer text-primary-300 hover:text-primary-200">
          <FaHome size={32} />
        </Link>

        <form
          onSubmit={handleSubmit}
          className={`relative flex items-center bg-primary-300 rounded-2xl opacity-80 duration-200
         hover:opacity-100 ${
           isFocused
             ? "border-[3px] border-primary-200"
             : "border-[3px] border-transparent"
         }`}
        >
          <Input
            className="px-2 py-4 w-72 text-primary-50 focus:outline-none placeholder-primary-100"
            placeholder="What do you want to play?"
            value={value}
            onChange={(e) => setValue(e.target.value)}
            onFocus={() => setIsFocused(true)}
            onBlur={() => setIsFocused(false)}
          />
          <span className="text-xl text-primary-100 absolute right-4 cursor-pointer">
            <FaSearch />
            {value.length > 0 && (
              <span
                className="text-xl text-primary-100 hover:text-red-500 absolute right-8 top-0 cursor-pointer"
                onClick={handleClearInput}
              >
                <FaTimes />
              </span>
            )}
          </span>
        </form>
      </div>

      <div className="flex items-center gap-4">
            {user ? (
              <button onClick={handleLogout} className="bg-primary-500 text-primary-100 px-4 py-2 rounded hover:bg-primary-700">
                Logout
              </button>
            ): (
              <>
                <Link to="/login" className="text-primary-300 hover:text-primary-100 px-4 py-2">
                  Login
                </Link>
                <Link to="/signup" className="bg-primary-300 text-primary-100 px-4 py-2 rounded hover:bg-primary-600">
                  Signup
                </Link>
              </>
            )}
      </div>
    </nav>
  );
};

export default Navbar;
