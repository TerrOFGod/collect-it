import {BadRequestException, HttpException, HttpStatus, Injectable, NotFoundException} from '@nestjs/common';
import {User} from "./users.model";
import {InjectModel} from "@nestjs/sequelize";
import {RolesService} from "../roles/roles.service";
import {Role} from "../roles/roles.model";

const emailRegex = /^[a-zA-Z\d.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z\d](?:[a-zA-Z\d-]{0,61}[a-zA-Z\d])?(?:\.[a-zA-Z\d](?:[a-zA-Z\d-]{0,61}[a-zA-Z\d])?)*$/;
const usernameRegex = /\w[\w\d]{5,}/;
@Injectable()
export class UsersService {

    constructor(@InjectModel(User) private usersRepository: typeof User,
                private rolesService: RolesService) { }


    async getAllUsersAsync(pageNumber: number, pageSize: number) {
        return await this.usersRepository.findAndCountAll({
            limit: pageSize,
            offset: (pageNumber - 1) * pageSize,
            include: Role,
            order: [['Id', 'ASC']]
        });
    }

    async getUserByEmailAsync(email: string) {
        return await this.usersRepository.findOne({
            where: {
                email: email,
            },
            include: Role
        });
    }

    async getUserByUsernameAsync(username: string) {
        return await this.usersRepository.findOne({
            where: {
                username: username,
            },
            include: [{
                all: true,
            }]
        });
    }

    async addRoleToUser(userId: number, roleName: string) {
        const user = await this.usersRepository.findByPk(userId);
        const role = await this.rolesService.getRoleByName(roleName);
        if (!(role && user)) {
            throw new NotFoundException('No user or role found');
        }
        await user.$add('role', role.id);
        await user.save();
    }

    async removeRoleFromUser(userId: number, roleName: string) {
        try {
            const user = await this.usersRepository.findByPk(userId);
            const role = await this.rolesService.getRoleByName(roleName);
            await user.$remove('role', role.id);
            await user.save();
        } catch (e) {
            console.log(e);
            throw new HttpException({
                message: 'Error while deleting role'
            }, HttpStatus.BAD_REQUEST);
        }
    }

    async getUserByIdAsync(userId: number) {
        return await this.usersRepository.findByPk(userId, {
            include: [{all: true}],
        });
    }

    async changeEmailAsync(userId: number, email: string) {
        if (!emailRegex.test(email)) {
            throw new Error('Email is not in correct form');
        }
        await this.usersRepository.update({email: email, normalizedEmail: email.toUpperCase()},
            {
                where: {
                    id: userId
                }
            });
    }

    async changeUsernameAsync(userId: number, username: string) {
        if (!usernameRegex.test(username)) {
            throw new BadRequestException({
                message: 'Username must consist of min 6 letters, only digits and letters, first character is letter'
            })
        }
        await this.usersRepository.update({username: username, normalizedUsername: username.toUpperCase()}, {
            where: {
                id: userId
            }
        });
    }
}
