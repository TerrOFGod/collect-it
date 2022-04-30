import React, { useEffect, useState} from 'react';
import User from "../../entities/user";
import Pagination from "../../UI/Pagination/Pagination";
import {UsersService} from "../../../services/UsersService";
import {useNavigate} from "react-router";
import SearchPanel from "../../UI/SearchPanel/SearchPanel";


const UsersList = () => {
    const pageSize = 10;
    const [users, setUsers] = useState<User[]>([]);
    const [maxPages, setMaxPages] = useState(0);
    const [loading, setLoading] = useState(true);
    useEffect(() => {
        UsersService.getUsersPagedAsync({pageSize, pageNumber: 1})
            .then(x => {
                setUsers(x.users);
                setMaxPages(Math.ceil(x.totalCount / pageSize));
                setLoading(false);
            })
    }, [])

    const downloadPageNumber = (pageNumber: number) => {
        setLoading(true);
        UsersService.getUsersPagedAsync({pageSize, pageNumber})
            .then(x => {
                setUsers(x.users);
                setLoading(false)
            });
    }

    const nav = useNavigate();
    const toEditUserPage = (id: number) => nav(`/users/${id}`);
    const onSearch = (q: string) => {
        const id = Number(q);
        if (!Number.isInteger(id)) {
            alert('Id must be an integer');
            return;
        }
        toEditUserPage(id);
    }
    return (
        <div className={'container mt-5'}>
            <SearchPanel onSearch={onSearch} placeholder={'Enter User id'}/>
            <div className='mt-5 mx-auto'>
                <table className={'usersTable table table-borderless table-light'}>
                    <thead>
                    <th className='firstRow usersRow'>
                        <td className='Cell idCell'>ID</td>
                        <td className='Cell nameCell'>Username</td>
                        <td className='Cell nameCell'>E-mail</td>
                        <td className='Cell'>Roles</td>
                        <td className='Cell'>Subscriptions</td>
                    </th>
                    </thead>
                    <tbody className='mx-auto mt-5 table-hover'>
                    {loading
                        ? <>Loading...</>
                        : users?.map(i =>
                            <tr onClick={() => toEditUserPage(i.id)} className='usersRow'>
                                <td className='Cell idCell'>{i.id}</td>
                                <td className='Cell nameCell'><div className={'bigtext'}> {i.username}</div></td>
                                <td className='Cell nameCell'>{i.email}</td>
                                <td className='Cell'>{i.roles}</td>
                                <td className='Cell'>{i.subscriptions}</td>
                            </tr>
                        )}
                    </tbody>
                </table>
                <Pagination totalPagesCount={maxPages} onPageChange={downloadPageNumber}/>
            </div>
        </div>
    );
};

export default UsersList;